#!/bin/bash
set -e
echo "[*] Deploying POSTGRES to remote server $(hostname)..."

source /tmp/variables.env
source /tmp/secrets.env
source $APP_PATH/functions.sh

createpaths_from_metadata "postgres"

# Update the worker-1 node with the postgres tag
if [[ "$(hostname)" == *"manager-1"* ]]; then
  echo "[*] Updating infra-1 node with postgres tag..."
  POSTGRES_NODE=$(docker node ls --format '{{.Hostname}}' | grep "infra-1")
  docker node update --label-add postgres=true $POSTGRES_NODE
fi

echo "[*] Deploying POSTGRES to remote server $(hostname)...DONE"