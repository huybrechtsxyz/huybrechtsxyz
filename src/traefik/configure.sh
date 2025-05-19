#!/bin/bash
set -e
echo "[*] Deploying TRAEFIK to remote server..."

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

createpath "/app/traefik"
createpath "/app/traefik/conf"
createpath "/app/traefik/data"
createpath "/app/traefik/logs"

envsubst \
  '${DOMAIN_DEV} ${ENVIRONMENT}' \
  < /app/traefik/conf/traefik-config.template.yml \
  > /app/traefik/conf/traefik-config.yml

echo "[*] Deploying TRAEFIK to remote server...DONE"
