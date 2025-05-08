#!/bin/bash
set -e

# Read PostgreSQL credentials from secrets files
# if [ -f "/run/secrets/APP_ROOT_USERNAME" ]; then
#   POSTGRES_USER=$(cat /run/secrets/APP_ROOT_USERNAME)
# else
#   echo "Error: POSTGRES_USER secret file not found!" >&2
#   exit 1
# fi
POSTGRES_USER="root"

if [ -f "/run/secrets/APP_ROOT_PASSWORD" ]; then
  POSTGRES_PASSWORD=$(cat /run/secrets/APP_ROOT_PASSWORD)
else
  echo "Error: POSTGRES_PASSWORD secret file not found!" >&2
  exit 1
fi

# Wait for PostgreSQL to be ready
echo "Waiting for PostgreSQL $POSTGRES_URL to start..."
TIMEOUT=60
while ! pg_isready -h $POSTGRES_URL -U "$POSTGRES_USER" -d "$POSTGRES_DB"; do
  TIMEOUT=$((TIMEOUT-1))
  if [ $TIMEOUT -le 0 ]; then
    echo "Error: PostgreSQL did not become ready in time!" >&2
    exit 1
  fi
  sleep 2
done

# Execute the SQL file
if [ -f /usr/local/bin/postgres-init.sql ]; then
  echo "Executing initialization script..."
  PGPASSWORD="$POSTGRES_PASSWORD" psql -h $POSTGRES_URL -U "$POSTGRES_USER" -d "$POSTGRES_DB" -f /usr/local/bin/postgres-init.sql
else
  echo "Initialization script not found!"
fi

echo "Initialization completed."
