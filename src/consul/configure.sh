#!/bin/bash
set -e
echo "[*] Deploying CONSUL to remote server..."

source /tmp/variables.env
source /tmp/secrets.env
source $APP_PATH_CONF/functions.sh

createpaths "consul"

echo "[*] Deploying CONSUL to remote server...DONE"