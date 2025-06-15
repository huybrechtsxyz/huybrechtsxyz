#!/bin/bash
set -euo pipefail

#source /tmp/app/initialize.env (set in pipeline)
export PRIVATE_IP="$PRIVATE_IP"
export MANAGER_IP="$MANAGER_IP"

cd /

update_system() {
  echo "[*] Updating system packages..."
  apt-get update -y
  DEBIAN_FRONTEND=noninteractive apt-get upgrade -y
}

install_private_key() {
  echo "[*] Installing uploaded private key..."
  mkdir -p ~/.ssh
  mv /root/.ssh/id_rsa_temp ~/.ssh/id_rsa
  chmod 600 ~/.ssh/id_rsa
  echo -e "Host *\n  StrictHostKeyChecking no\n" > ~/.ssh/config
}

configure_firewall() {
  echo "[*] Configuring firewall..."
  if ! command -v ufw &> /dev/null; then
  echo "[*] Installing UFW..."
  apt-get install -y ufw
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
  echo "[*] Enabling UFW..."
  echo "y" | ufw enable
  fi

  ufw reload
  ufw status verbose
}

mount_disks() {
  echo "[*] Preparing and mounting disk volumes..."
  : "${ENVIRONMENT:?Missing ENVIRONMENT}"
  : "${WORKSPACE:?Missing WORKSPACE}"
  : "${APP_PATH_CONF:?Missing APP_PATH_CONF}"
  : "${APP_PATH_DATA:?Missing APP_PATH_DATA}"
  : "${APP_PATH_LOGS:?Missing APP_PATH_LOGS}"
  : "${APP_PATH_SERV:?Missing APP_PATH_SERV}"

  # Get the current cluster definition
  echo "[*] ... Getting the cluster definition"
  HOSTNAME=$(hostname)
  CLUSTER_FILE="/tmp/app/cluster.$WORKSPACE.json"
  echo "[*] ... Finding cluster metadata file $CLUSTER_FILE on $HOSTNAME"
  if [ ! -f "$CLUSTER_FILE" ]; then
    echo "[!] Cluster metadata file not found: $CLUSTER_FILE on $HOSTNAME"
    return 1
  fi

  # Get the disk sizes for this server
  echo "[*] ... Getting disk sizes"
  SERVER_ID=$(jq -r '.servers[].id' "$CLUSTER_FILE" | while read id; do
    if [[ "$HOSTNAME" == *"$id"* ]]; then
      echo "$id"
      break
    fi
  done)

  if [ -z "$SERVER_ID" ]; then
    echo "[!] No matching server ID found for hostname $HOSTNAME"
    return 1
  fi

  DISK_SIZES=($(jq -r --arg id "$SERVER_ID" '.servers[] | select(.id == $id) | .disks[]?' "$CLUSTER_FILE"))

  # Map of path -> subdirectory
  DISK_INDEX=0
  DISK_LETTER_START="b"
  TMP_MOUNT="/mnt/appdisk"
  declare -A APP_PATHS_SECONDARY=(
    ["$APP_PATH_DATA"]="data"
    ["$APP_PATH_LOGS"]="logs"
    ["$APP_PATH_SERV"]="serv"
  )

  echo "[*] ... Mounting application disk"
  mkdir -p "$TMP_MOUNT"

  # Loop all disks for the current machine
  echo "[*] ... Looping disks"
  for SIZE in "${DISK_SIZES[@]}"; do
    # Start from /dev/sdb (ASCII 98 = 'b')
    DISK="/dev/sd$(echo "$DISK_INDEX" | awk '{printf("%c", 97 + $1)}')"
    PART="${DISK}1"

    echo "[*] ... Checking $DISK (size ${SIZE}GB)..."
    if [ ! -b "$DISK" ]; then
      echo "[!] Disk $DISK not found, skipping."
      DISK_INDEX=$((DISK_INDEX + 1))
      continue
    fi

    if [ $DISK_INDEX -eq 0 ]; then
      echo "[*] ... Using $DISK for configuration only (NO formatting / mounting)"
      echo "[*] ... Creating $APP_PATH_CONF"
      mkdir -p "$APP_PATH_CONF"
      mkdir -p "$APP_PATH_DATA"
      mkdir -p "$APP_PATH_LOGS"
      mkdir -p "$APP_PATH_SERV"
      DISK_INDEX=$((DISK_INDEX + 1))
      continue
    fi
    
    echo "[*] ... Preparing secondary disk $DISK"
    # Partition if missing
    if ! blkid "$PART" &>/dev/null; then
      echo "[*] ... Partitioning secondary disk $DISK"
      echo ',,L,*' | sfdisk "$DISK"
      partprobe "$DISK"
      sleep 2
    fi

    # Format
    echo "[*] ... Validation format of disk $DISK"
    FS_TYPE=$(blkid -o value -s TYPE "$PART" 2>/dev/null || echo "")
    if [ "$FS_TYPE" != "ext4" ]; then
      echo "[*] ... Formatting disk $DISK part $PART as ext4..."
      mkfs.ext4 -F "$PART"
    fi

    # Mount and create directories
    echo "[*] ... Mount validation disk $DISK"
    if ! mountpoint -q "$TMP_MOUNT"; then
      echo "[*] ... Mounting disk $DISK"
      mount "$PART" "$TMP_MOUNT" || return 1
      for sub in "${APP_PATHS_SECONDARY[@]}"; do
        echo "[*] ... Creating subdirectory $TMP_MOUNT/$sub"
        mkdir -p "$TMP_MOUNT/$sub"
      done
    fi

    echo "[*] ... Validate disk $DISK to /etc/fstab"
    UUID=$(blkid -s UUID -o value "$PART")
    if ! grep -q "UUID=$UUID" /etc/fstab; then
      echo "[*] ... Adding disk $DISK to /etc/fstab"
      echo "UUID=$UUID $TMP_MOUNT ext4 defaults 0 2" | tee -a /etc/fstab
    fi

    echo "[*] ... Mounting subdirectories to $TMP_MOUNT"
    for target in "${!APP_PATHS_SECONDARY[@]}"; do
      sub="${APP_PATHS_SECONDARY[$target]}"
      mkdir -p "$target"
      if ! grep -q "$TMP_MOUNT/$sub $target none bind" /etc/fstab; then
        echo "$TMP_MOUNT/$sub $target none bind 0 0" | tee -a /etc/fstab
      fi
      mount "$target" || { echo "[!] Failed to mount $target"; return 1; }
    done

    DISK_INDEX=$((DISK_INDEX + 1))
  done

  echo "[+] Preparing and mounting disk volumes...DONE"
}

install_docker_if_needed() {
  if ! command -v docker &> /dev/null; then
    echo "[*] Installing Docker..."
    apt-get update
    apt-get install -y ca-certificates curl gnupg lsb-release
    curl -fsSL https://get.docker.com | bash
  else
    echo "[*] Docker is already installed."
  fi
}

configure_swarm() {
  local hostname
  hostname=$(hostname)
  echo "[*] Configuring Docker Swarm on $hostname..."

  if [ "$(docker info --format '{{.Swarm.LocalNodeState}}' 2>/dev/null)" = "active" ]; then
    echo "[*] Node already part of a Swarm. Skipping initialization/joining."
    if [[ "$hostname" == *"manager-1"* ]]; then
      docker swarm join-token manager -q > /tmp/app/manager_token.txt
      docker swarm join-token worker -q > /tmp/app/worker_token.txt
    fi
    return
  fi

  if [[ "$hostname" == *"manager-1"* ]]; then
    echo "[*] Initializing new Swarm cluster..."
    docker swarm init --advertise-addr "$PRIVATE_IP"
    mkdir -p /tmp/app
    chmod 1777 /tmp/app
    docker swarm join-token manager -q > /tmp/app/manager_token.txt
    docker swarm join-token worker -q > /tmp/app/worker_token.txt
    echo "[*] Saved manager and worker join tokens."
  else
    echo "[*] Joining existing Swarm cluster on $MANAGER_IP..."
    SSH_OPTS="-o StrictHostKeyChecking=no -o ConnectTimeout=10"
    for i in {1..12}; do
      if ssh $SSH_OPTS root@$MANAGER_IP 'test -f /tmp/app/manager_token.txt && test -f /tmp/app/worker_token.txt'; then
        echo "[*] Swarm tokens are available on $MANAGER_IP"
        break
      fi
      echo "[!] Attempt $i: Waiting for Swarm tokens..."
      sleep 5
    done

    if ! ssh $SSH_OPTS root@$MANAGER_IP 'test -f /tmp/app/manager_token.txt && test -f /tmp/app/worker_token.txt'; then
      echo "[x] Timed out waiting for Swarm tokens. Exiting."
      exit 1
    fi

    MANAGER_JOIN_TOKEN=$(ssh $SSH_OPTS root@$MANAGER_IP 'cat /tmp/app/manager_token.txt')
    WORKER_JOIN_TOKEN=$(ssh $SSH_OPTS root@$MANAGER_IP 'cat /tmp/app/worker_token.txt')

    if [[ "$hostname" == *"manager-"* ]]; then
      echo "[*] Joining as Swarm Manager..."
      docker swarm join --token "$MANAGER_JOIN_TOKEN" $MANAGER_IP:2377 --advertise-addr "$PRIVATE_IP"
    else
      echo "[*] Joining as Swarm Worker..."
      docker swarm join --token "$WORKER_JOIN_TOKEN" $MANAGER_IP:2377 --advertise-addr "$PRIVATE_IP"
    fi

    echo "[*] Successfully joined Swarm cluster"
  fi
}

enable_docker_service() {
  echo "[*] Ensuring Docker is enabled and running..."

  # Enable docker service only if it's not already enabled
  if ! systemctl is-enabled --quiet docker; then
    echo "[*] Enabling Docker service..."
    systemctl enable docker
  else
    echo "[*] Docker service is already enabled."
  fi

  # Start docker service if not active
  if ! systemctl is-active --quiet docker; then
    echo "[*] Starting Docker service..."
    systemctl start docker
  else
    echo "[*] Docker service is already running."
  fi
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
