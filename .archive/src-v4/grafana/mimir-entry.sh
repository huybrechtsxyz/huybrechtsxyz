#!/bin/bash

# Export secrets as environment variables
export MINIO_ROOT_USER=$(cat /run/secrets/MINIO_ROOT_USER)
export MINIO_ROOT_PASSWORD=$(cat /run/secrets/MINIO_ROOT_PASSWORD)

# Now run the Grafana Mimir binary with the environment variables
exec /usr/bin/mimir \
  -config.file=/etc/mimir/config.yml \
  -storage.s3.access_key_id=${MINIO_ROOT_USER} \
  -storage.s3.secret_access_key=${MINIO_ROOT_PASSWORD} \
  -auth.multitenancy-enabled=false \
  -distributor.ring.instance-addr=$(hostname -i) \
  -ingester.ring.instance-addr=$(hostname -i)
