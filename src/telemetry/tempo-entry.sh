#!/bin/bash

# Export secrets as environment variables
export MINIO_ROOT_USER=$(cat /run/secrets/APP_ROOT_USERNAME)
export MINIO_ROOT_PASSWORD=$(cat /run/secrets/APP_ROOT_PASSWORD)

# Now run binary with the environment variables
exec /tempo -config.file=/etc/tempo-config.yml -config.expand-env=true
