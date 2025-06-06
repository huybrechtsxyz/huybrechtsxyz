#!/bin/bash
set -e
echo "[*] Deploying REDISINSIGHT to remote server..."

source /tmp/variables.env
source /tmp/secrets.env
source $APP_PATH/functions.sh

createpaths_from_metadata "redisinsight"

echo "[*] Deploying REDISINSIGHT to remote server...DONE"
