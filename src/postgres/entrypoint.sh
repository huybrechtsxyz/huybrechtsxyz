#!/bin/sh
set -e

if [ -f "/run/secrets/PLATFORM_PASSWORD" ]; then
  POSTGRES_PASSWORD=$(cat /run/secrets/PLATFORM_PASSWORD)
else
  echo "Error: POSTGRES_PASSWORD secret file not found!" >&2
  exit 1
fi

echo "Waiting for PostgreSQL $POSTGRES_URL to start..."

TIMEOUT=60

while ! pg_isready -h "$POSTGRES_URL" -U "$POSTGRES_USER" -d "$POSTGRES_DB"; do
  TIMEOUT=$(expr $TIMEOUT - 1)
  if [ "$TIMEOUT" -le 0 ]; then
    echo "Error: PostgreSQL did not become ready in time!" >&2
    exit 1
  fi
  sleep 2
done

if [ -f /usr/local/bin/initialize.sql ]; then
  echo "Executing initialization script..."
  PGPASSWORD="$POSTGRES_PASSWORD" psql -h "$POSTGRES_URL" -U "$POSTGRES_USER" -d "$POSTGRES_DB" -f /usr/local/bin/initialize.sql
else
  echo "Initialization script not found!"
fi

echo "Initialization completed."
