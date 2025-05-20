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

createpath "/srv/app/postgres"
createpath "/srv/app/postgres/conf"
createpath "/srv/app/postgres/data"
createpath "/srv/app/postgres/admin"
createpath "/srv/app/postgres/backups"

# Update the worker-1 node with the postgres tag
if [[ "$hostname" == *"manager-1"* ]]; then
  echo "[*] Updating worker-1 node with postgres tag..."
  POSTGRES_NODE=$(docker node ls --format '{{.Hostname}}' | grep "worker-1")
  docker node update --label-add postgres=true $POSTGRES_NODE
fi

echo "[*] Deploying POSTGRES to remote server...DONE"