#!/bin/bash
set -e
echo "[*] Deploying PGADMIN to remote server $(hostname)..."
source /tmp/variables.sh
source /tmp/secrets.env
source $APP_PATH_CONF/functions.sh
create_service_paths "pgadmin"
echo "[*] Deploying PGADMIN to remote server $(hostname)...DONE"