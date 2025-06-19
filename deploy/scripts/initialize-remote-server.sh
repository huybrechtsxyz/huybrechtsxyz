#!/bin/bash
set -euo pipefail

source "$(dirname "${BASH_SOURCE[0]}")/../../scripts/functions.sh"

# source /tmp/app/initialize.env (set in pipeline)
export PRIVATE_IP="$PRIVATE_IP"
export MANAGER_IP="$MANAGER_IP"

cd /

update_system() {
  log INFO "[*] Updating system packages..."
  apt-get update -y
  DEBIAN_FRONTEND=noninteractive apt-get upgrade -y
  log INFO "[+] Updating system packages...DONE"
}

install_private_key() {
  log INFO "[*] Installing uploaded private key..."
  mkdir -p ~/.ssh
  mv /root/.ssh/id_rsa_temp ~/.ssh/id_rsa
  chmod 600 ~/.ssh/id_rsa
  echo -e "Host *\n  StrictHostKeyChecking no\n" > ~/.ssh/config
  log INFO "[+] Installing uploaded private key...DONE"
}

configure_firewall() {
  log INFO "[*] Configuring firewall..."
  if ! command -v ufw &> /dev/null; then
  log INFO "[*] ... Installing UFW ..."
  apt-get install -y ufw
  log INFO "[*] ... Installing UFW ...DONE"
  fi

  # Deny all traffic by default
  ufw --force reset
  ufw default deny incoming
  ufw default deny outgoing

  # Essential System Services
  ufw allow out 53/tcp comment 'DNS (TCP)'
  ufw allow out 53/udp comment 'DNS (UDP)'
  ufw allow out 123/udp comment 'NTP'

  # Loopback
  ufw allow in on lo comment 'Loopback IN'
  ufw allow out on lo comment 'Loopback OUT'

  # Package Management (Optional)
  ufw allow out 20,21/tcp comment 'FTP'
  ufw allow out 11371/tcp comment 'GPG keyserver'

  # Web & SSH
  ufw allow 22/tcp comment 'SSH'
  ufw allow 80/tcp comment 'HTTP'
  ufw allow 443/tcp comment 'HTTPS'
  ufw allow out 80/tcp comment 'HTTP'
  ufw allow out 443/tcp comment 'HTTPS'

  # SSH Outbound to internal nodes
  ufw allow out proto tcp to 10.0.0.0/23 port 22 comment 'SSH Outbound to internal nodes'

  # Docker Swarm management traffic (TCP) over VLAN
  ufw allow proto tcp from 10.0.0.0/23 to any port 2377 comment 'Swarm Control IN'
  ufw allow out proto tcp to 10.0.0.0/23 port 2377 comment 'Swarm Control OUT'

  # Docker VXLAN overlay network (UDP) over VLAN
  ufw allow proto udp from 10.0.0.0/23 to any port 4789 comment 'Swarm Overlay Network IN'
  ufw allow out proto udp to 10.0.0.0/23 port 4789 comment 'Swarm Overlay Network OUT'

  # Docker overlay network discovery (TCP + UDP) over VLAN
  ufw allow proto tcp from 10.0.0.0/23 to any port 7946 comment 'Swarm Discovery TCP'
  ufw allow proto udp from 10.0.0.0/23 to any port 7946 comment 'Swarm Discovery UDP'
  ufw allow out proto tcp to 10.0.0.0/23 port 7946 comment 'Swarm Gossip TCP OUT'
  ufw allow out proto udp to 10.0.0.0/23 port 7946 comment 'Swarm Gossip UDP OUT'

  # Enable firewall only if not active
  if ! ufw status | grep -q "Status: active"; then
  log INFO "[*] Enabling UFW..."
  echo "y" | ufw enable
  fi

  ufw reload
  ufw status verbose
  log INFO "[+] Configuring firewall...DONE"
}

mount_disks() {
  log INFO "[*] Preparing and mounting disk volumes..."

  : "${WORKSPACE:?Missing WORKSPACE}"
  local hostname=$(hostname)
  local workspace_file="$PATH_TEMP/workspace.$WORKSPACE.json"

  if [[ ! -f "$workspace_file" ]]; then
    log ERROR "[!] Cluster metadata file not found: $workspace_file on $hostname"
    return 1
  fi

  local server_id
  server_id=$(jq -r '.servers[].id' "$workspace_file" | while read -r id; do
    [[ "$hostname" == *"$id"* ]] && echo "$id" && break
  done)

  if [[ -z "$server_id" ]]; then
    log ERROR "[!] No matching server ID found for hostname: $hostname"
    return 1
  fi

  # Get disks sorted by size (skipping the OS disk manually)
  mapfile -t disks < <(lsblk -dn -o NAME,SIZE -b | sort -k2,2n -k1,1)
  declare -a disk_names
  for line in "${disks[@]}"; do
    disk_names+=("$(echo "$line" | awk '{print $1}')")
  done

  local disk_count
  disk_count=$(jq -r --arg id "$server_id" '.servers[] | select(.id == $id) | .disks | length' "$workspace_file")
  log INFO "[*] ... Found $disk_count disks for $hostname"

  for i in $(seq 0 $((disk_count - 1))); do
    log INFO "[*] ... Mounting disk $i for $hostname"

    local disk="/dev/${disk_names[$i]}"
    local label
    label=$(jq -r --arg id "$server_id" --argjson i "$i" \
      '.servers[] | select(.id == $id) | .disks[$i].label' "$workspace_file")

    local part="${disk}1"
    local fs_type=$(blkid -s TYPE -o value "$part" 2>/dev/null || echo "")
    local current_label=$(blkid -s LABEL -o value "$part" 2>/dev/null || echo "")
    local mnt="/mnt/data${i}"

    log INFO "[*] ... Preparing disk $disk (label=$label)"

    if ! lsblk "$part" &>/dev/null; then
      log INFO "[*] ... Partitioning $disk"
      parted -s "$disk" mklabel gpt
      parted -s -a optimal "$disk" mkpart primary ext4 0% 100%
      sync
      sleep 2
      part="/dev/$(lsblk -nro NAME "$disk" | sed -n '2p')"
    else
      log INFO "[*] ... Skipping partitioning: $disk already partitioned"
    fi

    if [[ -z "$fs_type" ]]; then
      log INFO "[*] ... Formatting $part as ext4 with label $label"
      mkfs.ext4 -L "$label" "$part"
    elif [[ "$fs_type" != "ext4" ]]; then
      log WARN "[!] $part has unexpected FS type ($fs_type), skipping"
      continue
    elif [[ "$current_label" != "$label" ]]; then
      log INFO "[*] ... Relabeling $part from $current_label to $label"
      e2label "$part" "$label"
    else
      log INFO "[*] ... $part already formatted and labeled $label"
    fi

    mkdir -p "$mnt"
    if ! mountpoint -q "$mnt"; then
      log INFO "[*] Mounting $label to $mnt"
      mount "/dev/disk/by-label/$label" "$mnt"
    else
      log INFO "[+] Already mounted: $mnt"
    fi

    log INFO "[*] ... Mounting disk $i for $hostname DONE"
  done

  log INFO "[+] Preparing and mounting disk volumes...DONE"
}

install_docker_if_needed() {
  if ! command -v docker &> /dev/null; then
    log INFO "[*] Installing Docker..."
    apt-get update
    apt-get install -y ca-certificates curl gnupg lsb-release
    curl -fsSL https://get.docker.com | bash
    log INFO "[+] Installing Docker...DONE"
  else
    log INFO "[*] Docker is already installed."
  fi
}

configure_swarm() {
  local hostname
  hostname=$(hostname)
  log INFO "[*] Configuring Docker Swarm on $hostname..."

  if [ "$(docker info --format '{{.Swarm.LocalNodeState}}' 2>/dev/null)" = "active" ]; then
    log INFO "[*] Node already part of a Swarm. Skipping initialization/joining."
    if [[ "$hostname" == *"manager-1"* ]]; then
      docker swarm join-token manager -q > /tmp/app/manager_token.txt
      docker swarm join-token worker -q > /tmp/app/worker_token.txt
    fi
    return
  fi

  if [[ "$hostname" == *"manager-1"* ]]; then
    log INFO "[*] ... Initializing new Swarm cluster..."
    docker swarm init --advertise-addr "$PRIVATE_IP"
    mkdir -p /tmp/app
    chmod 1777 /tmp/app
    docker swarm join-token manager -q > /tmp/app/manager_token.txt
    docker swarm join-token worker -q > /tmp/app/worker_token.txt
    log INFO "[*] ... Saved manager and worker join tokens."
  else
    log INFO "[*] ... Joining existing Swarm cluster on $MANAGER_IP..."
    SSH_OPTS="-o StrictHostKeyChecking=no -o ConnectTimeout=10"
    for i in {1..12}; do
      if ssh $SSH_OPTS root@$MANAGER_IP 'test -f /tmp/app/manager_token.txt && test -f /tmp/app/worker_token.txt'; then
        log INFO "[*] ... Swarm tokens are available on $MANAGER_IP"
        break
      fi
      log WARN "[!] ... Attempt $idex: Waiting for Swarm tokens..."
      sleep 5
    done

    if ! ssh $SSH_OPTS root@$MANAGER_IP 'test -f /tmp/app/manager_token.txt && test -f /tmp/app/worker_token.txt'; then
      log ERROR "[x] Timed out waiting for Swarm tokens. Exiting."
      exit 1
    fi

    MANAGER_JOIN_TOKEN=$(ssh $SSH_OPTS root@$MANAGER_IP 'cat /tmp/app/manager_token.txt')
    WORKER_JOIN_TOKEN=$(ssh $SSH_OPTS root@$MANAGER_IP 'cat /tmp/app/worker_token.txt')

    if [[ "$hostname" == *"manager-"* ]]; then
      log INFO "[*] ... Joining as Swarm Manager..."
      docker swarm join --token "$MANAGER_JOIN_TOKEN" $MANAGER_IP:2377 --advertise-addr "$PRIVATE_IP"
    else
      log INFO "[*] ... Joining as Swarm Worker..."
      docker swarm join --token "$WORKER_JOIN_TOKEN" $MANAGER_IP:2377 --advertise-addr "$PRIVATE_IP"
    fi

    log INFO "[+] Successfully joined Swarm cluster"
  fi

  log INFO "[+] Configuring Docker Swarm on $hostname...DONE"
}

enable_docker_service() {
  log INFO "[*] Ensuring Docker is enabled and running..."

  # Enable docker service only if it's not already enabled
  if ! systemctl is-enabled --quiet docker; then
    log INFO "[*] ... Enabling Docker service..."
    systemctl enable docker
  else
    log INFO "[*] ... Docker service is already enabled."
  fi

  # Start docker service if not active
  if ! systemctl is-active --quiet docker; then
    log INFO "[*] ... Starting Docker service..."
    systemctl start docker
  else
    log INFO "[*] ... Docker service is already running."
  fi

  log INFO "[*] Ensuring Docker is enabled and running...DONE"
}

main() {
    echo "[*] Initializing remote server..."
    update_system || exit 1
    install_private_key || exit 1
    configure_firewall || exit 1
    mount_disks || exit 1
    install_docker_if_needed || exit 1
    configure_swarm || exit 1
    enable_docker_service || exit 1
    echo "[*] Remote server cleanup..."
    rm -rf /tmp/app/*
    echo "[+] Remote server initialization completed."
}

main
