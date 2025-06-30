#!/bin/bash
set -e

if [ -f "/run/secrets/PLATFORM_USERNAME" ]; then
  POSTGRES_USER=$(cat /run/secrets/PLATFORM_USERNAME)
else
  echo "Error: PLATFORM_USERNAME secret file not found!" >&2
  exit 1
fi

if [ -f "/run/secrets/PLATFORM_PASSWORD" ]; then
  POSTGRES_PASSWORD=$(cat /run/secrets/PLATFORM_PASSWORD)
else
  echo "Error: POSTGRES_PASSWORD secret file not found!" >&2
  exit 1
fi

# Wait for PostgreSQL to be ready
echo "Waiting for PostgreSQL $POSTGRES_HOST to start..."
TIMEOUT=60
while ! pg_isready -h $POSTGRES_HOST -U "$POSTGRES_USER" -d "$POSTGRES_DB"; do
  TIMEOUT=$((TIMEOUT-1))
  if [ $TIMEOUT -le 0 ]; then
    echo "Error: PostgreSQL did not become ready in time!" >&2
    exit 1
  fi
  sleep 2
done

echo 'Postgres is ready!' &&
touch /pgready/pgready &&
echo 'pgready file created.'"
