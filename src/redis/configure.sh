#!/bin/bash
set -e
echo "[*] Deploying REDIS to remote server $(hostname)..."
create_service_paths "redis"
echo "[*] Deploying REDIS to remote server $(hostname)...DONE"
