#!/bin/bash
set -e

# Timestamp for backup
TIMESTAMP=$(date +"%Y%m%d%H%M%S")

# Read secrets for PostgreSQL
export POSTGRES_USER=$(cat /run/secrets/POSTGRES_USER)
export POSTGRES_PASSWORD=$(cat /run/secrets/POSTGRES_PASSWORD)
export POSTGRES_HOST=${POSTGRES_HOST:-"postgres"}

# Backup PostgreSQL Databases
echo "Backing up PostgreSQL databases..."
if pg_dumpall -U $POSTGRES_USER -h $POSTGRES_HOST > /backups/postgres-backup-$TIMESTAMP.sql; then
  echo "PostgreSQL backup completed."
else
  echo "PostgreSQL backup failed." >&2
  exit 1
fi

# Set MinIO client alias
echo "Configuring MinIO client..."
export MINIO_ROOT_USER=$(cat /run/secrets/MINIO_ROOT_USER)
export MINIO_ROOT_PASSWORD=$(cat /run/secrets/MINIO_ROOT_PASSWORD)
mc alias set myminio http://minio:9000 $MINIO_ROOT_USER $MINIO_ROOT_PASSWORD

# Upload PostgreSQL backup to MinIO
echo "Uploading PostgreSQL backup to MinIO..."
if mc cp /backups/postgres-backup-$TIMESTAMP.sql myminio/backups/; then
  echo "Backup uploaded successfully."
else
  echo "Backup upload to MinIO failed." >&2
  exit 1
fi

echo "Backup completed successfully."
