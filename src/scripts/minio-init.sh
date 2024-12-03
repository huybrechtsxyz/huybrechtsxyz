#!/bin/bash

# Function to create bucket if it doesn't exist and set the read-write policy
create_bucket_if_not_exists() {
  local bucket_name=$1
  echo "Checking if bucket $bucket_name exists..."
  
  if ! mc ls myminio/$bucket_name > /dev/null 2>&1; then
    echo "Creating bucket $bucket_name..."
    mc mb myminio/$bucket_name || { echo "Failed to create bucket $bucket_name"; exit 1; }
    echo "Created bucket $bucket_name"
  else
    echo "Bucket $bucket_name already exists"
  fi
  
  echo "Setting read-write policy for $bucket_name bucket..."
  mc anonymous set public myminio/$bucket_name || { echo "Failed to set policy for $bucket_name"; exit 1; }
  echo "Set read-write policy for $bucket_name bucket"
}

# Main script execution
echo "Setting MinIO alias..."
export MINIO_ROOT_USER=$(cat $MINIO_ROOT_USER_FILE)
export MINIO_ROOT_PASSWORD=$(cat $MINIO_ROOT_PASSWORD_FILE)

echo "MinIO Root User: $MINIO_ROOT_USER"
echo "MinIO Root Password: $MINIO_ROOT_PASSWORD"

mc alias set myminio http://minio:9000 $MINIO_ROOT_USER $MINIO_ROOT_PASSWORD

# Create and configure buckets
create_bucket_if_not_exists backups
# Uncomment the following lines if you want to create other buckets
# create_bucket_if_not_exists thanos
# create_bucket_if_not_exists loki