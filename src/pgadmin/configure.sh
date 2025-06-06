#!/bin/bash
set -e
echo "[*] Deploying PGADMIN to remote server $(hostname)..."

source /tmp/variables.env
source /tmp/secrets.env
source $APP_PATH/functions.sh

createpaths_from_metadata "pgadmin"

echo "[*] Deploying PGADMIN to remote server $(hostname)...DONE"