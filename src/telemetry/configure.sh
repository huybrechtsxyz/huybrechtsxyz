#!/bin/bash
set -e
echo "[*] Deploying TELEMETRY to remote server..."

source /tmp/variables.env
source /tmp/secrets.env
source $APP_PATH/functions.sh

createpaths_from_metadata "telemetry"

echo "[*] Deploying TELEMETRY to remote server...DONE"