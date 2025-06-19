#!/bin/bash
set -e
echo "[*] Deploying TRAEFIK to remote server $(hostname)..."

echo ENVIRONMENT: $ENVIRONMENT
export ENVIRONMENT=$ENVIRONMENT

echo DOMAIN_DEV: $DOMAIN_DEV
export DOMAIN_DEV=$DOMAIN_DEV

envsubst \
  < "$TRAEFIK_PATH_CONF/traefik/config.template.yml" \
  > "$TRAEFIK_PATH_CONF/traefik/config.yml"

echo "[*] Deploying TRAEFIK to remote server $(hostname)...DONE"
