#!/bin/bash

set -euo pipefail

echo "[*] Initializing server..."
set -euo pipefail
cd /

install_private_key() {
  echo "[*] Installing uploaded private key..."
  mkdir -p ~/.ssh
  mv /root/.ssh/id_rsa_temp ~/.ssh/id_rsa
  chmod 600 ~/.ssh/id_rsa
  echo -e "Host *\n  StrictHostKeyChecking no\n" > ~/.ssh/config
}

update_system() {
  echo "[*] Updating system packages..."
  apt-get update -y
  DEBIAN_FRONTEND=noninteractive apt-get upgrade -y
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

mount_disks() {
  echo "[*] Preparing and mounting disk volumes..."
  : "${ENVIRONMENT:?Missing ENVIRONMENT}"
  : "${APP_PATH_CONF:?Missing APP_PATH_CONF}"
  : "${APP_PATH_DATA:?Missing APP_PATH_DATA}"
  : "${APP_PATH_LOGS:?Missing APP_PATH_LOGS}"
  : "${APP_PATH_SERV:?Missing APP_PATH_SERV}"

  # Get the current cluster definition
  echo "[*] ... Getting the cluster definition"
  HOSTNAME=$(hostname)
  CLUSTER_FILE="/tmp/cluster.$ENVIRONMENT.json"
  echo "[*] ... Finding cluster metadata file $CLUSTER_FILE"
  if [ ! -f "$CLUSTER_FILE" ]; then
    echo "[!] Cluster metadata file not found: $CLUSTER_FILE"
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
  sudo mkdir -p "$TMP_MOUNT"

  # Loop all disks for the current machine
  echo "[*] ... Looping disks"
  for SIZE in "${DISK_SIZES[@]}"; do
    # Start from /dev/sdb (ASCII 98 = 'b')
    DISK="/dev/sd$(echo "$DISK_INDEX" | awk '{printf("%c", 98 + $1)}')"
    PART="${DISK}1"

    echo "[*] Checking $DISK (size ${SIZE}GB)..."
    if [ ! -b "$DISK" ]; then
      echo "[!] Disk $DISK not found, skipping."
      ((DISK_INDEX++))
      continue
    fi

    if [ $DISK_INDEX -eq 0 ]; then
      echo "[+] Using $DISK for APP_PATH_CONF (NO formatting)"
      sudo mount "$PART" "$TMP_MOUNT" || return 1
      sudo mkdir -p "$TMP_MOUNT/conf"
      UUID=$(sudo blkid -s UUID -o value "$PART")
      echo "UUID=$UUID $TMP_MOUNT ext4 defaults 0 2" | sudo tee -a /etc/fstab
      echo "$TMP_MOUNT/conf $APP_PATH_CONF none bind 0 0" | sudo tee -a /etc/fstab
      sudo mkdir -p "$APP_PATH_CONF"
      sudo mount "$APP_PATH_CONF"
    else
      echo "[+] Preparing secondary disk $DISK for data/logs/serv"
      # Partition if missing
      if ! sudo blkid "$PART" &>/dev/null; then
        echo ',,L,*' | sudo sfdisk "$DISK"
        sudo partprobe "$DISK"
        sleep 2
      fi

      # Format
      FS_TYPE=$(sudo blkid -o value -s TYPE "$PART" 2>/dev/null || echo "")
      if [ "$FS_TYPE" != "ext4" ]; then
        echo "[*] Formatting $PART as ext4..."
        sudo mkfs.ext4 -F "$PART"
      fi

      # Mount and create directories
      sudo mount "$PART" "$TMP_MOUNT" || return 1
      for sub in "${APP_PATHS_SECONDARY[@]}"; do
        sudo mkdir -p "$TMP_MOUNT/$sub"
      done

      UUID=$(sudo blkid -s UUID -o value "$PART")
      if ! grep -q "UUID=$UUID" /etc/fstab; then
        echo "UUID=$UUID $TMP_MOUNT ext4 defaults 0 2" | sudo tee -a /etc/fstab
      fi

      for target in "${!APP_PATHS_SECONDARY[@]}"; do
        sub="${APP_PATHS_SECONDARY[$target]}"
        sudo mkdir -p "$target"
        if ! grep -q "$TMP_MOUNT/$sub $target none bind" /etc/fstab; then
          echo "$TMP_MOUNT/$sub $target none bind 0 0" | sudo tee -a /etc/fstab
        fi
        sudo mount "$target" || { echo "[!] Failed to mount $target"; return 1; }
      done
    fi

    ((DISK_INDEX++))
  done

  echo "[+] Preparing and mounting disk volumes...DONE"
}

main() {
  update_system
  configure_firewall
  mount_disks
  install_private_key
  install_docker_if_needed
}

main
