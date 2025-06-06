#!/bin/bash
set -euo pipefail

REMOTE_IP="$1"
ENVIRONMENT="$2"

echo "[*] Copying secrets to remote server..."

# Write secrets to local temp file
cat <<EOF > /tmp/secrets.env
PLATFORM_USERNAME=${PLATFORM_USERNAME}
PLATFORM_PASSWORD=${PLATFORM_PASSWORD}
TRAEFIK_CLIENTID=${TRAEFIK_CLIENTID}
TRAEFIK_SECRET=${TRAEFIK_SECRET}
VERSIO_USERNAME=${VERSIO_USERNAME}
VERSIO_PASSWORD=${VERSIO_PASSWORD}
EOF

# Transfer secrets and env files
scp -o StrictHostKeyChecking=no /tmp/secrets.env root@"$REMOTE_IP":/tmp/secrets.env

echo "[*] Copying environment files to remote server..."
scp -o StrictHostKeyChecking=no ./src/test.env root@"$REMOTE_IP":/tmp/vars-test.env
scp -o StrictHostKeyChecking=no ./src/staging.env root@"$REMOTE_IP":/tmp/vars-staging.env
scp -o StrictHostKeyChecking=no ./src/production.env root@"$REMOTE_IP":/tmp/vars-production.env
scp -o StrictHostKeyChecking=no ./src/shared.env root@"$REMOTE_IP":/tmp/vars-shared.env
scp -o StrictHostKeyChecking=no ./src/"$ENVIRONMENT".env root@"$REMOTE_IP":/tmp/variables.env

echo "[*] Copying configuration scripts..."
scp -o StrictHostKeyChecking=no ./deploy/scripts/configure-begin.sh root@"$REMOTE_IP":/tmp/configure-begin.sh
scp -o StrictHostKeyChecking=no ./deploy/scripts/configure-end.sh root@"$REMOTE_IP":/tmp/configure-end.sh

echo "[*] Executing configuration script remotely..."
ssh -o StrictHostKeyChecking=no root@"$REMOTE_IP" << 'EOF'
  chmod +x /tmp/configure-begin.sh
  /tmp/configure-begin.sh
  cp -f /tmp/variables.env /opt/app/.env
  cp -f /tmp/vars-test.env /opt/app/test.env
  cp -f /tmp/vars-staging.env /opt/app/staging.env
  cp -f /tmp/vars-production.env /opt/app/production.env
  cp -f /tmp/vars-shared.env /opt/app/shared.env
EOF

echo "[*] Copying scripts to /opt/app on remote server..."
scp -o StrictHostKeyChecking=no ./scripts/*.sh root@"$REMOTE_IP":/opt/app
