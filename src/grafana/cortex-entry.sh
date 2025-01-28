!/bin/bash

# Export secrets as environment variables
export MINIO_ROOT_USER=$(cat /run/secrets/MINIO_ROOT_USER)
export MINIO_ROOT_PASSWORD=$(cat /run/secrets/MINIO_ROOT_PASSWORD)

# Now run binary with the environment variables
exec /bin/cortex \
    -config.file=/config/cortex-config.yml \
    -blocks-storage.s3.access-key-id=${MINIO_ROOT_USER} \
    -blocks-storage.s3.secret-access-key=${MINIO_ROOT_PASSWORD}
