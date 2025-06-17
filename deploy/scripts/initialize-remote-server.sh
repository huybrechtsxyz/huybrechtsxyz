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

# Function to convert size string like "20G" or "1024M" to GB integer (approx)
size_to_gb() {
  local size=$1
  if [[ "$size" =~ ^([0-9]+)([GMK])$ ]]; then
    local val=${BASH_REMATCH[1]}
    local unit=${BASH_REMATCH[2]}
    case $unit in
      G) echo "$val" ;;
      M) echo $((val / 1024)) ;;
      K) echo 0 ;;
      *) echo 0 ;;
    esac
  else
    echo 0
  fi
}

mount_disks() {
  echo "[*] Preparing and mounting disk volumes..."
  : "${ENVIRONMENT:?Missing ENVIRONMENT}"
  : "${WORKSPACE:?Missing WORKSPACE}"
  : "${APP_PATH_CONF:?Missing APP_PATH_CONF}"
  : "${APP_PATH_DATA:?Missing APP_PATH_DATA}"
  : "${APP_PATH_LOGS:?Missing APP_PATH_LOGS}"
  : "${APP_PATH_SERV:?Missing APP_PATH_SERV}"
  : "${APP_PATH_TEMP:?Missing APP_PATH_TEMP}"

  HOSTNAME=$(hostname)
  echo "[*] ... Getting the workspace definition for $HOSTNAME"
  WORKSPACE_FILE="$APP_PATH_TEMP/workspace.$WORKSPACE.json"
  echo "[*] ... Finding cluster metadata file $WORKSPACE_FILE on $HOSTNAME"
  if [ ! -f "$WORKSPACE_FILE" ]; then
    echo "[!] Cluster metadata file not found: $WORKSPACE_FILE on $HOSTNAME"
    return 1
  fi

  echo "[*] ... Identifying matching server config"
  SERVER_ID=$(jq -r --arg hn "$HOSTNAME" '.servers[] | select($hn | contains(.id)) | .id' "$WORKSPACE_FILE")
  if [ -z "$SERVER_ID" ]; then
    echo "[!] No matching server ID found for hostname $HOSTNAME"
    return 1
  fi

  DISK_SIZES=($(jq -r --arg id "$SERVER_ID" '.servers[] | select(.id == $id) | .disks[]?' "$WORKSPACE_FILE"))
  if [ ${#DISK_SIZES[@]} -le 1 ]; then
    echo "[*] No additional disks found to mount."
    echo "[*] Creating default paths on the OS disk."
    mkdir -p "$APP_PATH_CONF" "$APP_PATH_DATA" "$APP_PATH_LOGS" "$APP_PATH_SERV"
    return 0
  fi

  # List all non-OS disks (sorted by size asc, stable order)
  echo "[*] ... Getting real disks from server $HOSTNAME"
  mapfile -t DISKS < <(lsblk -dn -o NAME,SIZE -b | sort -k2,2n -k1,1)
  declare -a DISK_NAMES
  for line in "${DISKS[@]}"; do
    NAME=$(echo "$line" | awk '{print $1}')
    DISK_NAMES+=("$NAME")
  done

  if [ "${#DISK_SIZES[@]}" -gt "${#DISK_NAMES[@]}" ]; then
    echo "[!] Not enough disks found on host to match expected workspace config"
    return 1
  fi

  echo "[*] ... Updating profile"
  mkdir -p /mnt
  profile_file="/etc/profile.d/app_paths.sh"
  echo "[*] ... Auto-generated application paths" > "$profile_file"

  for idex in "${!DISK_SIZES[@]}"; do
    SIZE_EXPECTED=${DISK_SIZES[$idex]}
    if [ "$idex" -eq 0 ]; then
      echo "[*] ... Validating disk $idex as OS disk (assumed mounted on /)"
      mkdir -p "$APP_PATH_CONF"
      echo "[*] ... Skipping disk $idex as OS disk (assumed mounted on /)"
      continue
    fi

    jdex=$idex #$((idex - 1)) -> no counter reset
    DISK="/dev/${DISK_NAMES[$idex]}"
    LABEL="${SERVER_ID}-data${jdex}"
    PART=$(lsblk -nro NAME "$DISK" | tail -n +2 | head -n1)
    PART="/dev/$PART"

    echo "[*] ... Preparing disk $DISK (label=$LABEL)"
    if ! blkid "$PART" &>/dev/null; then
      echo "[*] ... Partitioning $DISK"
      parted -s "$DISK" mklabel gpt
      parted -s -a optimal "$DISK" mkpart primary ext4 0% 100%
      sleep 2
      PART=$(lsblk -nro NAME "$DISK" | tail -n +2 | head -n1)
      PART="/dev/$PART"
    else
      echo "[*] ... Skipping partitioning $DISK. Already partitioned."
    fi

    FS_TYPE=$(blkid -s TYPE -o value "$PART" || echo "")
    CURRENT_LABEL=$(blkid -s LABEL -o value "$PART" || echo "")

    if [ -z "$FS_TYPE" ]; then
      echo "[*] Formatting $PART as ext4 with label $LABEL"
      mkfs.ext4 -L "$LABEL" "$PART"
      sync
    elif [ "$FS_TYPE" != "ext4" ]; then
      echo "[!] $PART has unexpected FS type ($FS_TYPE), skipping."
      continue
    elif [ "$CURRENT_LABEL" != "$LABEL" ]; then
      echo "[*] Relabeling $PART from $CURRENT_LABEL to $LABEL"
      e2label "$PART" "$LABEL"
    else
      echo "[*] $PART already formatted and labeled $LABEL"
    fi
    
    UUID=$(blkid -s UUID -o value "$PART")
    MOUNT_POINT="/mnt/data$jdex"
    mkdir -p "$MOUNT_POINT"

    if ! grep -q "^UUID=$UUID " /etc/fstab; then
      echo "UUID=$UUID $MOUNT_POINT ext4 defaults,noatime,nofail 0 2" >> /etc/fstab
    fi

    if ! mountpoint -q "$MOUNT_POINT"; then
      mount "$MOUNT_POINT"
      sync
    fi

    # Bind directories
    DATA_PATH="${APP_PATH_DATA}${jdex}/"
    LOGS_PATH="${APP_PATH_LOGS}${jdex}/"
    SERV_PATH="${APP_PATH_SERV}${jdex}/"

    mkdir -p "$MOUNT_POINT/data" "$MOUNT_POINT/logs" "$MOUNT_POINT/serv"
    mkdir -p "$DATA_PATH" "$LOGS_PATH" "$SERV_PATH"

    for src in data logs serv; do
      SRC_PATH="$MOUNT_POINT/$src"
      case "$src" in
        data) DST_PATH="/var/lib/data$jdex" ;;
        logs) DST_PATH="/var/lib/logs$jdex" ;;
        serv) DST_PATH="/srv/app$jdex" ;;
      esac
      if ! grep -q "^$SRC_PATH $DST_PATH " /etc/fstab; then
        echo "$SRC_PATH $DST_PATH none bind 0 0" >> /etc/fstab
      fi
      mount --bind "$SRC_PATH" "$DST_PATH"
    done

    for var in DATA LOGS SERV; do
      VAR_NAME="APP_PATH_${var}${jdex}"
      VAR_VALUE="/var/lib/$(echo "$var" | tr '[:upper:]' '[:lower:]')$jdex"
      if ! grep -q "export $VAR_NAME=" "$profile_file"; then
        echo "export $VAR_NAME=$VAR_VALUE" >> "$profile_file"
      fi
    done
  done

  chmod +x "$profile_file"

  echo "[*] Disk setup complete. Paths exported to $profile_file"
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
      echo "[!] Attempt $idex: Waiting for Swarm tokens..."
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
