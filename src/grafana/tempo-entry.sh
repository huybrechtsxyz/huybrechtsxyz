!/bin/bash

# Export secrets as environment variables
export MINIO_ROOT_USER=$(cat /run/secrets/MINIO_ROOT_USER)
export MINIO_ROOT_PASSWORD=$(cat /run/secrets/MINIO_ROOT_PASSWORD)

# Now run binary with the environment variables
exec /bin/loki \
    -config=/etc/loki/loki-config.yml \
    -config.expand-env=true
