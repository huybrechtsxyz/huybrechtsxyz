#!/bin/bash
set -e
echo "[*] Deploying POSTGRES to remote server $(hostname)..."

source /tmp/variables.env
source /tmp/secrets.env
source $APP_PATH_CONF/functions.sh

createpaths "postgres"

echo "[*] Deploying POSTGRES to remote server $(hostname)...DONE"