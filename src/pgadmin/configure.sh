#!/bin/bash
set -e
echo "[*] Deploying PGADMIN to remote server $(hostname)..."
create_service_paths "pgadmin"
echo "[*] Deploying PGADMIN to remote server $(hostname)...DONE"