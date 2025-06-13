#!/bin/bash
set -e
echo "[*] Deploying KEYCLOAK to remote server $(hostname)..."
source /tmp/variables.sh
source /tmp/secrets.env
source $APP_PATH_CONF/functions.sh
create_service_paths "keycloak"
echo "[*] Deploying KEYCLOAK to remote server $(hostname)...DONE"