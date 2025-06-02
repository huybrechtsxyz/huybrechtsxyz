#!/bin/bash
set -e
echo "[*] Deploying MINIO to remote server..."

source /tmp/variables.env
source /tmp/secrets.env
source $APP_PATH/functions.sh

createpaths_from_metadata "minio"

# Update the worker-1 node with the postgres tag
if [[ "$hostname" == *"manager-1"* ]]; then
  echo "[*] Updating infra-2 node with minio tag..."
  MINIO_NODE=$(docker node ls --format '{{.Hostname}}' | grep "infra-2")
  docker node update --label-add minio=true $MINIO_NODE
fi

echo "[*] Deploying MINIO to remote server...DONE"