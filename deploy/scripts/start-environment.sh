#!/bin/bash
set -euo pipefail

NODE_NAME="$1"
NODE_IP="$2"
SERVICES="$3"

if [[ "$NODE_NAME" != *"manager-1"* ]]; then
  echo "[*] Environments are only started on leader node."
  exit 0
fi

if [[ -z "$SERVICES" ]]; then
  echo "[*] No services selected for startup."
  exit 0
fi

echo "[*] Starting Environment on $NODE_NAME ($NODE_IP)..."

ssh -o StrictHostKeyChecking=no root@"$NODE_IP" << EOF
  cd /opt/app
  ./remove.sh $SERVICES
  sleep 30
  ./deploy.sh $SERVICES
EOF

echo "[*] Starting Environment...DONE"