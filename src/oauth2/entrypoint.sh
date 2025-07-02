#!/bin/sh
set -e

# Load functions for secrets and substitution
if [ -f /etc/libutils.sh ]; then
  . /etc/libutils.sh
else
  echo "ERROR . /etc/libutils.sh not found"
  exit 1
fi

# Read all _FILE secret files to environment variables
load_secret_files

# Convert the config.template.cfg file for this instance
export OAUTH2_CLIENTID=$OAUTH2_CLIENTID
export OAUTH2_SECRET=$OAUTH2_SECRET
export OAUTH2_COOKIE=$OAUTH2_COOKIE

export OIDC_ISSUER_URL=$OIDC_ISSUER_URL
export COOKIE_DOMAIN=$COOKIE_DOMAIN
export REDIRECT_HOST=$REDIRECT_HOST
export WHITELIST_DOMAIN=$WHITELIST_DOMAIN

substitute_env_vars /etc/config.template.cfg /etc/config.cfg

echo "Generated /etc/config.cfg:"
cat /etc/config.cfg

exec oauth2-proxy --config=/etc/config.cfg
