#!/bin/bash
set -e
echo "[*] Deploying KEYCLOAK to remote server..."

source /tmp/variables.env
source /tmp/secrets.env
source $APP_PATH_CONF/functions.sh

createpaths "keycloak"

echo Environment: $ENVIRONMENT
export ENVIRONMENT=$ENVIRONMENT

echo "[*] Deploying KEYCLOAK to remote server...DONE"