#!/bin/bash
set -e
echo "[*] Deploying PGADMIN to remote server $(hostname)..."

source /tmp/variables.env
source /tmp/secrets.env
source $APP_PATH_CONF/functions.sh

createpaths "pgadmin"

echo "[*] Deploying PGADMIN to remote server $(hostname)...DONE"