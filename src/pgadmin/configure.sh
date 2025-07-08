#!/bin/bash
set -e
echo "[*] Deploying PGADMIN to remote server $(hostname)..."

PATH_CONF=$1
echo PATH_CONF: $PATH_CONF

if [ -z "$OAUTH2_PGADMIN_SECRET" ]; then
  echo "Error: OAUTH2_PGADMIN_SECRET is not set."
  exit 1
else
  echo OAUTH2_PGADMIN_SECRET: ***
fi
export OAUTH2_PGADMIN_SECRET=$OAUTH2_PGADMIN_SECRET

echo ENVIRONMENT: $ENVIRONMENT
export ENVIRONMENT=$ENVIRONMENT

echo DOMAIN_DEV: $DOMAIN_DEV
export DOMAIN_DEV=$DOMAIN_DEV

echo REALM_ID: $REALM_ID
export REALM_ID=$REALM_ID

envsubst \
  < "$PATH_CONF/pgadmin/config/config.template.py" \
  > "$PATH_CONF/pgadmin/config/config_local.py"

echo "[*] Deploying PGADMIN to remote server $(hostname)...DONE"