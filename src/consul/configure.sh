#!/bin/bash
set -e
echo "[*] Deploying CONSUL to remote server $(hostname)..."
create_service_paths "consul"
echo "[*] Deploying CONSUL to remote server $(hostname)...DONE"