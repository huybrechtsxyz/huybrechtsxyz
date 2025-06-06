#!/bin/bash

set -euo pipefail

echo "[*] Initializing server..."
set -euo pipefail
cd /

enable_docker_service() {
  echo "[*] Ensuring Docker is enabled and running..."
  systemctl enable docker
  systemctl restart docker
}

main() {
  enable_docker_service
}

main