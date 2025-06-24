#!/bin/bash
set -e
echo "[*] Deploying TRAEFIK to remote server $(hostname)..."

PATH_CONF=$1
echo PATH_CONF: $PATH_CONF

echo ENVIRONMENT: $ENVIRONMENT
export ENVIRONMENT=$ENVIRONMENT

echo DOMAIN_DEV: $DOMAIN_DEV
export DOMAIN_DEV=$DOMAIN_DEV

envsubst \
  < "$PATH_CONF/traefik/config/config.template.yml" \
  > "$PATH_CONF/traefik/config/config.yml"

echo "[*] Deploying TRAEFIK to remote server $(hostname)...DONE"
