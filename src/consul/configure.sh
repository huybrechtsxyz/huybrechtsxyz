#!/bin/bash
set -e
echo "[*] Deploying CONSUL to remote server $(hostname)..."
source /tmp/variables.env
source /tmp/secrets.env
source $APP_PATH_CONF/functions.sh
create_service_paths "consul"
echo "[*] Deploying CONSUL to remote server $(hostname)...DONE"