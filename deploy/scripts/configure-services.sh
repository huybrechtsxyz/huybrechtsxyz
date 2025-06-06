#!/bin/bash
set -euo pipefail

REMOTE_IP="$1"

echo "[*] Configuring application services..."

for script in ./src/*/configure.sh; do
  service=$(basename "$(dirname "$script")")
  echo "[*] Configuring service '$service'..."

  remote_conf_dir="/opt/app/$service/conf"
  remote_script="$remote_conf_dir/configure.sh"

  # Ensure remote conf directory exists
  ssh -o StrictHostKeyChecking=no root@"$REMOTE_IP" "mkdir -p '$remote_conf_dir'"

  # Copy service config files
  scp -o StrictHostKeyChecking=no ./src/$service/*.* root@"$REMOTE_IP":"$remote_conf_dir/" || {
    echo "[x] Failed to transfer $service configuration to remote server"
    exit 1
  }

  # Run the config script remotely
  ssh -o StrictHostKeyChecking=no root@"$REMOTE_IP" \
    "chmod +x '$remote_script' && bash '$remote_script'" || {
      echo "[x] Configuration for $service failed"
      exit 1
    }

  echo "[V] Configured service '$service'"
done

echo "[*] Configuring application services...DONE"
