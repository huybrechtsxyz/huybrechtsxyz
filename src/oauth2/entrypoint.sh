#!/bin/sh
set -e

# Load functions for secrets and substitution
if [ -f /etc/functions.sh ]; then
  . /etc/functions.sh
else
  echo "ERROR . /etc/functions.sh not found"
  exit 1
fi

# Read all _FILE secret files to environment variables
load_secret_files

# Convert the config.template.cfg file for this instance
export OIDC_ISSUER_URL=$OIDC_ISSUER_URL
export OAUTH2_CLIENT_ID=$OAUTH2_CLIENT_ID
export OAUTH2_CLIENT_SECRET=$OAUTH2_CLIENT_SECRET
export OAUTH2_COOKIE_SECRET=$OAUTH2_COOKIE_SECRET
export COOKIE_DOMAIN=$COOKIE_DOMAIN
export REDIRECT_HOST=$REDIRECT_HOST
export WHITELIST_DOMAIN=$WHITELIST_DOMAIN

for secret_var in OAUTH2_CLIENT_ID OAUTH2_CLIENT_SECRET OAUTH2_COOKIE_SECRET; do
  val="$(eval echo \$$secret_var)"
  if [ -n "$val" ]; then
    echo "$secret_var = ***"
  else
    echo "$secret_var is not set"
  fi
done

substitute_env_vars /etc/config.template.cfg /etc/config.cfg

echo "Generated /etc/config.cfg:"
cat /etc/config.cfg

exec oauth2-proxy --config=/etc/config.cfg
