#!/bin/bash
set -e
echo "[*] Deploying TRAEFIK to remote server $(hostname)..."

echo PATH_CONF: $PATH_CONF

echo ENVIRONMENT: $ENVIRONMENT
export ENVIRONMENT=$ENVIRONMENT

echo DOMAIN_DEV: $DOMAIN_DEV
export DOMAIN_DEV=$DOMAIN_DEV

envsubst \
  < "$PATH_CONF/traefik/config.template.yml" \
  > "$PATH_CONF/traefik/config.yml"

echo "[*] Deploying TRAEFIK to remote server $(hostname)...DONE"
