#!/bin/bash
set -euo pipefail

PRIVATE_KEY="$1"

echo "[*] Setting up SSH key..."
mkdir -p ~/.ssh
echo "$PRIVATE_KEY" > ~/.ssh/id_rsa
chmod 600 ~/.ssh/id_rsa
echo "[*] SSH setup complete."
