#!/bin/bash
set -e
echo "[*] Deploying TRAEFIK to remote server $(hostname)..."

source $APP_PATH_CONF/$ENVIRONMENT.env
source $APP_PATH_CONF/secrets.env
source $APP_PATH_CONF/functions.sh

create_service_paths "traefik"

echo ENVIRONMENT: $ENVIRONMENT
export ENVIRONMENT=$ENVIRONMENT
echo DOMAIN_DEV: $ENVIRONMENT
export DOMAIN_DEV=$DOMAIN_DEV
envsubst \
  < "$APP_PATH_CONF/traefik/config.template.yml" \
  > "$APP_PATH_CONF/traefik/config.yml"

echo "[*] Deploying TRAEFIK to remote server $(hostname)...DONE"