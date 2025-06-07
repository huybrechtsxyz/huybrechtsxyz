#!/bin/bash
set -e
echo "[*] Deploying KEYCLOAK to remote server..."

source /tmp/variables.env
source /tmp/secrets.env
source $APP_PATH/functions.sh

createpaths_from_metadata "keycloak"

echo Environment: $ENVIRONMENT
export ENVIRONMENT=$ENVIRONMENT

echo "[*] Deploying KEYCLOAK to remote server...DONE"