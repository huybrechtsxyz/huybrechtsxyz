#!/bin/bash
set -e
echo "[*] Deploying SEAWEED to remote server $(hostname)..."

source /tmp/variables.env
source /tmp/secrets.env
source $APP_PATH/functions.sh

createpaths_from_metadata "seaweed"

echo "[*] Deploying SEAWEED to remote server $(hostname)...DONE"