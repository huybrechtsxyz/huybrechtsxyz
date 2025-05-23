#!/bin/bash
set -e
echo "[*] Deploying KEYCLOAK to remote server..."

source /tmp/variables.env
source /tmp/secrets.env
source $APP_PATH/functions.sh

createpaths_from_metadata "consul"

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