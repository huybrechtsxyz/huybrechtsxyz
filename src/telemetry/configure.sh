#!/bin/bash
set -e
echo "[*] Deploying TELEMETRY to remote server..."

source /tmp/variables.env
source /tmp/secrets.env
source $APP_PATH/functions.sh

createpaths_from_metadata "consul"

echo "[*] Deploying TELEMETRY to remote server...DONE"