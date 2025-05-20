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

createpath "/srv/app/keycloak"
createpath "/srv/app/keycloak/conf"

envsubst < \
  /srv/app/keycloak/conf/keycloak-realm.template.json > \
  /srv/app/keycloak/conf/keycloak-realm.json

echo "[*] Deploying KEYCLOAK to remote server...DONE"