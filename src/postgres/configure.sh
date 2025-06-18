#!/bin/bash
set -e
echo "[*] Deploying POSTGRES to remote server $(hostname)..."
source $APP_PATH_CONF/$ENVIRONMENT.env
source $APP_PATH_CONF/secrets.env
source $APP_PATH_CONF/functions.sh
create_service_paths "postgres"
echo "[*] Deploying POSTGRES to remote server $(hostname)...DONE"