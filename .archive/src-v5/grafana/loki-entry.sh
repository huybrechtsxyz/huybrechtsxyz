#!/bin/bash

# Exit immediately if a command exits with a non-zero status
set -e

# Export secrets as environment variables
export MINIO_ROOT_USER=$(cat /run/secrets/MINIO_ROOT_USER)
export MINIO_ROOT_PASSWORD=$(cat /run/secrets/MINIO_ROOT_PASSWORD)

# Now run Loki with the environment variables
exec /usr/bin/loki \
    -config.file=/etc/loki/loki-config.yml \
    -config.expand-env=true
