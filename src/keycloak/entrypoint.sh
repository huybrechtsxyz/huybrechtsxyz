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

# Wait for Postgres readiness
echo "[INFO] Waiting for Postgres to be ready..."
while [ ! -f /pgready/pgready ]; do
  echo "[INFO] /pgready/pgready not found, sleeping 2s..."
  sleep 2
done
echo "[INFO] Postgres is ready!"

# Pass all command parameters
exec /opt/keycloak/bin/kc.sh start "$@"
