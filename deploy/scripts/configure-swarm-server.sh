#!/bin/bash
set -euo pipefail

REMOTE_IP="$1"
ENVIRONMENT="$2"

# Write secrets to local temp file
echo "[*] Copying secrets to remote server..."
cat <<EOF > /tmp/secrets.env
PLATFORM_USERNAME=${PLATFORM_USERNAME}
PLATFORM_PASSWORD=${PLATFORM_PASSWORD}
TRAEFIK_CLIENTID=${TRAEFIK_CLIENTID}
TRAEFIK_SECRET=${TRAEFIK_SECRET}
VERSIO_USERNAME=${VERSIO_USERNAME}
VERSIO_PASSWORD=${VERSIO_PASSWORD}
EOF

scp -o StrictHostKeyChecking=no /tmp/secrets.env root@"$REMOTE_IP":/tmp/secrets.env
echo "[+] Copying secrets to remote server...DONE"

echo "[*] Copying environment files to remote server..."
#scp -o StrictHostKeyChecking=no ./src/test.env root@"$REMOTE_IP":/tmp/vars-test.env
#scp -o StrictHostKeyChecking=no ./src/staging.env root@"$REMOTE_IP":/tmp/vars-staging.env
#scp -o StrictHostKeyChecking=no ./src/production.env root@"$REMOTE_IP":/tmp/vars-production.env
scp -o StrictHostKeyChecking=no ./src/shared.env root@"$REMOTE_IP":/tmp/vars-shared.env
scp -o StrictHostKeyChecking=no ./src/"$ENVIRONMENT".env root@"$REMOTE_IP":/tmp/variables.env
echo "[+] Copying environment files to remote server...DONE"

echo "[*] Copying configuration scripts..."
scp -o StrictHostKeyChecking=no ./deploy/scripts/configure-begin.sh root@"$REMOTE_IP":/tmp/configure-begin.sh
scp -o StrictHostKeyChecking=no ./deploy/scripts/configure-end.sh root@"$REMOTE_IP":/tmp/configure-end.sh
echo "[+] Copying configuration scripts...DONE"

echo "[*] Executing configuration script remotely..."
ssh -o StrictHostKeyChecking=no root@"$REMOTE_IP" << 'EOF'
  chmod +x /tmp/configure-begin.sh
  /tmp/configure-begin.sh
EOF
echo "[+] Executing configuration script remotely...DONE"

echo "[*] Copying scripts to $APP_PATH_CONF on remote server..."
scp -o StrictHostKeyChecking=no ./scripts/*.sh root@"$REMOTE_IP":"${{ env.APP_PATH_CONF }}/"
echo "[+] Copying scripts to $APP_PATH_CONF on remote server...DONE"