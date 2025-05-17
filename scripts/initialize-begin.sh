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

mount_data_volume() {
    echo "[*] Mounting data volume..."
    DISK="/dev/sdb"
    PART="${DISK}1"
    MOUNTPOINT="/mnt/data"

    # 1. Ensure the partition exists and is the right size
    if ! lsblk -no NAME "${PART}" &>/dev/null; then
        echo "Partition not found. Creating partition table and partition..."
        echo ',,L,*' | sudo sfdisk "${DISK}"
        sudo partprobe "${DISK}"
    else
        echo "Partition ${PART} already exists."
    fi

    # 2. Ensure the partition is formatted as ext4
    FS_TYPE=$(sudo blkid -o value -s TYPE "${PART}" 2>/dev/null || echo "")
    if [ "$FS_TYPE" != "ext4" ]; then
        echo "Formatting ${PART} as ext4..."
        sudo mkfs.ext4 -F "${PART}"
    else
        echo "${PART} is already formatted as ext4."
    fi

    # 3. Ensure the mount point exists
    sudo mkdir -p "${MOUNTPOINT}"

    # 4. Ensure the partition is mounted
    if ! mountpoint -q "${MOUNTPOINT}"; then
        echo "Mounting ${PART} to ${MOUNTPOINT}..."
        sudo mount "${PART}" "${MOUNTPOINT}"
    else
        echo "${MOUNTPOINT} is already a mount point."
    fi

    # 5. Ensure /etc/fstab entry exists for persistent mounting
    FSTAB_ENTRY="${PART} ${MOUNTPOINT} ext4 defaults 0 0"
    if ! grep -qs "^${PART} " /etc/fstab; then
        echo "Adding entry to /etc/fstab..."
        echo "${FSTAB_ENTRY}" | sudo tee -a /etc/fstab
    else
        echo "Entry for ${PART} already exists in /etc/fstab."
    fi

    echo "Disk setup complete."
}

main() {
    update_system
    configure_firewall
    mount_data_volume
    install_private_key
    install_docker_if_needed
}

main