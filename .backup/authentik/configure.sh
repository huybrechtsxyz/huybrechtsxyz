#!/bin/bash
set -e
echo "[*] Deploying AUTHENTIK to remote server..."

source /tmp/variables.env
source /tmp/secrets.env
source $APP_PATH/functions.sh

createpaths_from_metadata "authentik"

echo "[*] Deploying AUTHENTIK to remote server...DONE"
