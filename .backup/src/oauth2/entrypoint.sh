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

# Read all environment variables
export OAUTH2_PROXY_COOKIE_SECRET=$OAUTH2_TRAEFIK_COOKIE

export OAUTH2_PROXY_CLIENT_SECRET_FILE=$OAUTH2_PROXY_CLIENT_SECRET_FILE
export OAUTH2_PROXY_OIDC_ISSUER_URL=$OAUTH2_PROXY_OIDC_ISSUER_URL
export OAUTH2_PROXY_REDIRECT_URL=$OAUTH2_PROXY_REDIRECT_URL
export OAUTH2_PROXY_UPSTREAMS=$OAUTH2_PROXY_UPSTREAMS
export OAUTH2_PROXY_CLIENT_ID=$OAUTH2_PROXY_CLIENT_ID
export OAUTH2_PROXY_COOKIE_NAME=$OAUTH2_PROXY_COOKIE_NAME
export OAUTH2_PROXY_COOKIE_DOMAINS=$OAUTH2_PROXY_COOKIE_DOMAINS
export OAUTH2_PROXY_WHITELIST_DOMAINS=$OAUTH2_PROXY_WHITELIST_DOMAINS

exec oauth2-proxy \
  --config=/etc/config.cfg \
  --insecure-oidc-skip-issuer-verification
