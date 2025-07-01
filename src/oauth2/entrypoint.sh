#!/bin/sh
set -e

# Load function library if any
if [ -f /opt/oauth2/functions.sh ]; then
  . /opt/oauth2/functions.sh
fi

# Convert *_FILE env vars to env vars with contents
load_secret_files

for secret_var in OAUTH2_CLIENT_ID OAUTH2_CLIENT_SECRET OAUTH2_COOKIE_SECRET; do
  val="$(eval echo \$$secret_var)"
  if [ -n "$val" ]; then
    echo "$secret_var = ***"
  else
    echo "$secret_var is not set"
  fi
done

# Generate config file dynamically
cat >/etc/oauth2-proxy.cfg <<EOF
# OAuth2 Proxy Configuration

provider = oidc
oidc_issuer_url = ${OIDC_ISSUER_URL}
client_id = ${OAUTH2_CLIENT_ID:-oauth2-proxy}
client_secret = ${OAUTH2_CLIENT_SECRET}
cookie_secret = ${OAUTH2_COOKIE_SECRET}
cookie_secure = true
cookie_domain = ${COOKIE_DOMAIN}
http_address = 0.0.0.0:4180
redirect_url = https://${REDIRECT_HOST}/oauth2/callback
scope = openid email profile
session_cookie_name = _oauth2_proxy
session_store_type = cookie
session_lifetime = 8h
session_refresh = 1h
request_logging = true
log_level = info
whitelist_domains = ${WHITELIST_DOMAIN}
skip_provider_button = true
enable_refresh_tokens = true

EOF

echo "Generated /etc/oauth2-proxy.cfg:"
cat /etc/oauth2-proxy.cfg

# Base command
CMD="oauth2-proxy --config=/etc/oauth2-proxy.cfg"

# Append any arguments passed to the script (if any)
if [ ! -z "$1" ]; then
  CMD="$CMD $*"
fi

# Exec the command (replaces shell process)
exec $CMD
