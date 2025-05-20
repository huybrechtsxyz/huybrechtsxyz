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

createpath "/opt/app/traefik"
createpath "/opt/app/traefik/conf"
createpath "/opt/app/traefik/data"
createpath "/opt/app/traefik/logs"

echo Environment: $ENVIRONMENT
EXPORT ENVIRONMENT=$ENVIRONMENT

echo Environment: $ENVIRONMENT
EXPORT DOMAIN_DEV=$DOMAIN_DEV

envsubst \
  < /opt/app/traefik/conf/traefik-config.template.yml \
  > /opt/app/traefik/conf/traefik-config.yml

echo "[*] Deploying TRAEFIK to remote server...DONE"
