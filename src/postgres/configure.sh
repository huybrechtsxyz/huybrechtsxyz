#!/bin/bash
set -e
echo "[*] Deploying POSTGRES to remote server..."

source /tmp/variables.env
source /tmp/secrets.env

# Function to create a path if it does not already exist
createpath() {
  local newpath="$1"

  if [ ! -d "$newpath" ]; then
    echo "[*] Creating directory: $newpath"
    if ! mkdir -p "$newpath"; then
      echo "[x] Error: Failed to create directory '$newpath'"
      return 1
    fi
  fi

  echo "[*] Setting permissions on $newpath"
  sudo chmod -R 777 "$newpath"

  return 0
}

createpath "/opt/app/postgres"
createpath "/opt/app/postgres/conf"
createpath "/opt/app/postgres/data"
createpath "/opt/app/postgres/admin"
createpath "/opt/app/postgres/backups"

# Update the worker-1 node with the postgres tag
if [[ "$hostname" == *"infra-1"* ]]; then
  echo "[*] Updating infra-1 node with postgres tag..."
  POSTGRES_NODE=$(docker node ls --format '{{.Hostname}}' | grep "worker-1")
  docker node update --label-add postgres=true $POSTGRES_NODE
fi

echo "[*] Deploying POSTGRES to remote server...DONE"