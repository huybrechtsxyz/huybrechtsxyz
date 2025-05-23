#!/bin/bash
set -e
echo "[*] Deploying CONSUL to remote server..."

source /tmp/variables.env
source /tmp/secrets.env
source $APP_PATH/functions.sh

createpaths_from_metadata "consul"

echo "[*] Deploying CONSUL to remote server...DONE"