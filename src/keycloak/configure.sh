#!/bin/bash
set -e
echo "[*] Deploying KEYCLOAK to remote server..."

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

createpath "/opt/app/keycloak"
createpath "/opt/app/keycloak/conf"

echo Environment: $ENVIRONMENT
export ENVIRONMENT=$ENVIRONMENT

echo Traefik Client ID: $TRAEFIK_CLIENTID
export TRAEFIK_CLIENTID=$TRAEFIK_CLIENTID

if [ -z "$TRAEFIK_SECRET" ]; then
  echo "Traefik Secret: TRAEFIK_SECRET is empty or not set"
else
  echo "Traefik Secret: TRAEFIK_SECRET is set"
fi
export TRAEFIK_SECRET=$TRAEFIK_SECRET

envsubst \
  < /opt/app/keycloak/conf/keycloak-realm.template.json \
  > /opt/app/keycloak/conf/keycloak-realm.json

echo "[*] Deploying KEYCLOAK to remote server...DONE"