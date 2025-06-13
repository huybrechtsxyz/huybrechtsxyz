#!/bin/bash
set -e
echo "[*] Deploying REDIS to remote server $(hostname)..."
source /tmp/variables.env
source /tmp/secrets.env
source $APP_PATH_CONF/functions.sh
create_service_paths "redis"
echo "[*] Deploying REDIS to remote server $(hostname)...DONE"
