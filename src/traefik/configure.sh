#!/bin/bash
set -e
echo "[*] Deploying TRAEFIK to remote server..."

source /tmp/variables.env
source /tmp/secrets.env
source $APP_PATH/functions.sh

createpaths "traefik"

echo ENVIRONMENT: $ENVIRONMENT
export ENVIRONMENT=$ENVIRONMENT

echo DOMAIN_DEV: $ENVIRONMENT
export DOMAIN_DEV=$DOMAIN_DEV

envsubst \
  < /opt/app/traefik/conf/config.template.yml \
  > /opt/app/traefik/conf/config.yml

echo "[*] Deploying TRAEFIK to remote server...DONE"
