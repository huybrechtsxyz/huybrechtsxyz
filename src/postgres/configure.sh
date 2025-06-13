#!/bin/bash
set -e
echo "[*] Deploying POSTGRES to remote server $(hostname)..."
create_service_paths "postgres"
echo "[*] Deploying POSTGRES to remote server $(hostname)...DONE"