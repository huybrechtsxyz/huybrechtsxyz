#!/bin/bash
set -e
echo "[*] Deploying PGADMIN to remote server $(hostname)..."
source $APP_PATH_CONF/$ENVIRONMENT.env
source $APP_PATH_CONF/secrets.env
source $APP_PATH_CONF/functions.sh
create_service_paths "pgadmin"
echo "[*] Deploying PGADMIN to remote server $(hostname)...DONE"