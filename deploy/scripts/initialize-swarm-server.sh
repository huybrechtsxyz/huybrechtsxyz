#!/bin/bash
set -euo pipefail

REMOTE_IP="$1"
PRIVATE_IP="$2"
MANAGER_IP="$3"

echo "[*] Copying swarm initialization script to remote server..."
scp -o StrictHostKeyChecking=no deploy/scripts/initialize-remote-server.sh root@"$REMOTE_IP":/tmp/remote-swarm-init.sh
echo "[+] Copying swarm initialization script to remote server...DONE"

echo "[*] Executing swarm initialization on $REMOTE_IP..."
ssh -o StrictHostKeyChecking=no root@"$REMOTE_IP" bash <<EOF
  chmod +x /tmp/remote-swarm-init.sh
  export PRIVATE_IP="$PRIVATE_IP"
  export MANAGER_IP="$MANAGER_IP"
  /tmp/remote-swarm-init.sh
EOF
echo "[+] Swarm initialization completed for $REMOTE_IP"
