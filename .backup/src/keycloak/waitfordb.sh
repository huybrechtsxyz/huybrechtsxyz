#!/bin/sh
set -e

# Remove stale readiness file if it exists
if [ -f /pgready/pgready ]; then
  echo "[INFO] Removing stale /pgready/pgready file..."
  rm -f /pgready/pgready
fi

if [ -f "/run/secrets/PLATFORM_PASSWORD" ]; then
  POSTGRES_PASSWORD=$(cat /run/secrets/PLATFORM_PASSWORD)
  export PGPASSWORD=$POSTGRES_PASSWORD
else
  echo "Error: POSTGRES_PASSWORD secret file not found!" >&2
  exit 1
fi

echo "Waiting for PostgreSQL $POSTGRES_HOST to start..."

TIMEOUT=60

while ! pg_isready -h "$POSTGRES_HOST" -U "$POSTGRES_USER" -d "$POSTGRES_DB" > /dev/null 2>&1; do
  TIMEOUT=$(expr $TIMEOUT - 1)
  if [ "$TIMEOUT" -le 0 ]; then
    echo "Error: PostgreSQL did not become ready in time!" >&2
    exit 1
  fi
  echo "Postgres unavailable - sleeping 2 seconds..."
  sleep 2
done

echo 'Postgres is ready!' &&
touch /pgready/pgready &&
echo 'pgready file created.'
