#!/bin/bash
set -e
echo "[*] Deploying REDIS to remote server..."

source /tmp/variables.env
source /tmp/secrets.env
source $APP_PATH/functions.sh

createpaths_from_metadata "redis"

echo "[*] Deploying REDIS to remote server...DONE"
