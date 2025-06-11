#!/bin/bash
set -e
echo "[*] Deploying REDISINSIGHT to remote server..."

source /tmp/variables.env
source /tmp/secrets.env
source $APP_PATH_CONF/functions.sh

createpaths "redisinsight"

echo "[*] Deploying REDISINSIGHT to remote server...DONE"
