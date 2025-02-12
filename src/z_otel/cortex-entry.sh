#!/bin/bash

# Loop until Consul returns a non-empty leader value (e.g. "172.20.0.2:8300")
echo "Waiting for Consul to be active..."
until leader=$(wget -q -O - http://consul:8500/v1/status/leader) && [ -n "$leader" ]; do
  echo "Consul not active yet. Waiting..."
  sleep 1
done
echo "Consul is active with leader: $leader"

# Export secrets as environment variables
export MINIO_ROOT_USER=$(cat /run/secrets/APP_ROOT_USERNAME)
export MINIO_ROOT_PASSWORD=$(cat /run/secrets/APP_ROOT_PASSWORD)

# Now run binary with the environment variables
/bin/cortex -config.file=/config/cortex-config.yml -config.expand-env=true
