#!/bin/bash
set -euo pipefail

REMOTE_IP="$1"
PRIVATE_IP="$2"
MANAGER_IP="$3"

# Check if required environment variables are set
: "${APP_PATH_CONF:?Missing APP_PATH_CONF}"
: "${APP_PATH_DATA:?Missing APP_PATH_DATA}"
: "${APP_PATH_LOGS:?Missing APP_PATH_LOGS}"
: "${APP_PATH_SERV:?Missing APP_PATH_SERV}"
: "${APP_PATH_TEMP:?Missing APP_PATH_TEMP}"
: "${ENVIRONMENT:?Missing ENVIRONMENT}"
: "${WORKSPACE:?Missing WORKSPACE}"
: "${REMOTE_IP:?Missing REMOTE_IP}"
: "${PRIVATE_IP:?Missing PRIVATE_IP}"
: "${MANAGER_IP:?Missing MANAGER_IP}"

# Create .env file for initialization
cat <<EOF > ./deploy/scripts/initialize.env
export APP_PATH_CONF=${APP_PATH_CONF}
export APP_PATH_DATA=${APP_PATH_DATA}
export APP_PATH_LOGS=${APP_PATH_LOGS}
export APP_PATH_SERV=${APP_PATH_SERV}
export APP_PATH_TEMP=${APP_PATH_TEMP}
export ENVIRONMENT=${ENVIRONMENT}
export WORKSPACE=${WORKSPACE}
export PRIVATE_IP="$PRIVATE_IP"
export MANAGER_IP="$MANAGER_IP"
EOF

echo "[*] Copying initialization script to remote server..."
ssh -o StrictHostKeyChecking=no root@$REMOTE_IP << EOF
  mkdir -p "$APP_PATH_TEMP"
  chmod 777 "$APP_PATH_TEMP"
EOF

echo "[*] Copying initialization scripts and cluster config to remote server..."
scp -o StrictHostKeyChecking=no \
  ./deploy/scripts/* \
  ./deploy/cluster."$WORKSPACE".json \
  root@"$REMOTE_IP":"$APP_PATH_TEMP"/ || {
    echo "[x] Failed to transfer initialization scripts to remote server"
    exit 1
  }

echo "[*] Executing REMOTE initialization..."
if ! ssh -o StrictHostKeyChecking=no root@"$REMOTE_IP" << EOF
  set -e
  echo "[*] Executing on REMOTE server..."
  set -a
  source "$APP_PATH_TEMP/initialize.env"
  set +a
  chmod +x "$APP_PATH_TEMP/initialize-remote-server.sh"
  "$APP_PATH_TEMP/initialize-remote-server.sh"
  echo "[*] Executing on REMOTE server...DONE"
EOF
then
  echo "[!] Remote initialization failed on $REMOTE_IP"
  exit 1
fi
echo "[+] Initialization complete."