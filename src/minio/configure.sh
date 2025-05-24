#!/bin/bash
set -e
echo "[*] Deploying MINIO to remote server..."

source /tmp/variables.env
source /tmp/secrets.env
source $APP_PATH/functions.sh

createpaths_from_metadata "minio"

echo "[*] Deploying MINIO to remote server...DONE"