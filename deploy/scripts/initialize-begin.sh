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

  DISK="/dev/sdb"
  PART="${DISK}1"
  : "${APP_PATH_CONF:?Missing APP_PATH_CONF}"
  : "${APP_PATH_DATA:?Missing APP_PATH_DATA}"
  : "${APP_PATH_LOGS:?Missing APP_PATH_LOGS}"
  : "${APP_PATH_SERV:?Missing APP_PATH_SERV}"

  declare -A APP_PATHS=(
    ["$APP_PATH_CONF"]="conf"
    ["$APP_PATH_DATA"]="data"
    ["$APP_PATH_LOGS"]="logs"
    ["$APP_PATH_SERV"]="serv"
  )

  # Wait for disk
  for i in {1..10}; do
    if [ -b "$DISK" ]; then
      echo "[+] ... Found disk ${DISK}"
      break
    fi
    echo "[*] ... Waiting for ${DISK}... ($i/10)"
    sleep 1
  done

  if [ ! -b "$DISK" ]; then
    echo "[!] Disk ${DISK} not found."
    return 1
  fi

  # Partition if missing
  if ! lsblk -no NAME "$PART" &>/dev/null; then
    echo "[*] .. Creating partition..."
    echo ',,L,*' | sudo sfdisk "$DISK"
    sudo partprobe "$DISK"
    sleep 2
  fi

  # Format if not ext4
  FS_TYPE=$(sudo blkid -o value -s TYPE "$PART" 2>/dev/null || echo "")
  if [ "$FS_TYPE" != "ext4" ]; then
    echo "[*] ... Formatting $PART as ext4..."
    sudo mkfs.ext4 -F "$PART"
  else
    echo "[+] ... $PART already formatted as ext4."
  fi

  TMP_MOUNT="/mnt/appdisk"
  sudo mkdir -p "$TMP_MOUNT"
  sudo mount "$PART" "$TMP_MOUNT"

  echo "[*] ... Creating app subdirectories..."
  for sub in "${APP_PATHS[@]}"; do
    sudo mkdir -p "$TMP_MOUNT/$sub"
  done

  UUID=$(sudo blkid -s UUID -o value "$PART")
  if [ -z "$UUID" ]; then
    echo "[!] Failed to get UUID."
    sudo umount "$TMP_MOUNT"
    return 1
  fi

  # Add base mount to fstab if missing
  if ! grep -q "$TMP_MOUNT" /etc/fstab; then
    echo "UUID=$UUID $TMP_MOUNT ext4 defaults 0 2" | sudo tee -a /etc/fstab
  fi

  # Mount again for binding
  sudo umount "$TMP_MOUNT"
  sudo mount "$TMP_MOUNT"  
  echo "[*] ... Bind-mounting app paths..."
  for target in "${!APP_PATHS[@]}"; do
    sub="${APP_PATHS[$target]}"
    sudo mkdir -p "$target"
    echo "$TMP_MOUNT/$sub $target none bind 0 0" | sudo tee -a /etc/fstab
    sudo mount "$target"
  done

  echo "[+] Disk volume setup complete."
}

main() {
  update_system
  configure_firewall
  mount_disks
  install_private_key
  install_docker_if_needed
}

main
