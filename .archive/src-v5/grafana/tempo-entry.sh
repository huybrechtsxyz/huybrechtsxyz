!/bin/bash

# Export secrets as environment variables
export MINIO_ROOT_USER=$(cat /run/secrets/MINIO_ROOT_USER)
export MINIO_ROOT_PASSWORD=$(cat /run/secrets/MINIO_ROOT_PASSWORD)

# Now run binary with the environment variables
exec /usr/bin/loki \
    -config.file=/etc/loki/loki-config.yaml \
    -config.expand-env=tru
