#!/bin/bash
set -e
echo "[*] Deploying BITWARDEN to remote server..."

source /tmp/variables.env
source /tmp/secrets.env
source $APP_PATH/functions.sh

createpaths_from_metadata "bitwarden"

echo "[*] Deploying BITWARDEN to remote server...DONE"
