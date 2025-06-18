#!/bin/bash
set -euo pipefail

if [ "$#" -ne 3 ]; then
  log ERROR "Usage: $0 <REMOTE_IP> <PRIVATE_IP> <MANAGER_IP>"
  exit 1
fi

REMOTE_IP="$1"
PRIVATE_IP="$2"
MANAGER_IP="$3"

# Check if required environment variables are set
check_variables() {
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
}

# Create .env file for initialization
create_env_file() {
cat <<EOF > ./deploy/scripts/initialize.env
export APP_PATH_CONF="${APP_PATH_CONF}"
export APP_PATH_DATA="${APP_PATH_DATA}"
export APP_PATH_LOGS="${APP_PATH_LOGS}"
export APP_PATH_SERV="${APP_PATH_SERV}"
export APP_PATH_TEMP="${APP_PATH_TEMP}"
export ENVIRONMENT="${ENVIRONMENT}"
export WORKSPACE="${WORKSPACE}"
export PRIVATE_IP="$PRIVATE_IP"
export MANAGER_IP="$MANAGER_IP"
EOF
}

copy_config_files() {
log INFO "[*] Copying initialization script to remote server..."
ssh -o StrictHostKeyChecking=no root@$REMOTE_IP << EOF
  mkdir -p "$APP_PATH_TEMP"
  chmod 777 "$APP_PATH_TEMP"
EOF

log INFO "[*] Copying initialization scripts and cluster config to remote server..."
scp -o StrictHostKeyChecking=no \
  ./deploy/scripts/* \
  ./deploy/workspace."$WORKSPACE".json \
  ./scripts/functions.sh \
  root@"$REMOTE_IP":"$APP_PATH_TEMP"/ || {
    echo "[x] Failed to transfer initialization scripts to remote server"
    exit 1
  }
}

execute_initialization() {
log INFO "[*] Executing REMOTE initialization..."
if ! ssh -o StrictHostKeyChecking=no root@"$REMOTE_IP" << EOF
  set -e
  echo "[*] Executing on REMOTE server..."
  set -a
  source "$APP_PATH_TEMP/initialize.env"
  source "$APP_PATH_TEMP/functions.sh"
  set +a
  chmod +x "$APP_PATH_TEMP/initialize-remote-server.sh"
  "$APP_PATH_TEMP/initialize-remote-server.sh"
  echo "[*] Executing on REMOTE server...DONE"
EOF
then
  log ERROR "[!] Remote initialization failed on $REMOTE_IP"
  exit 1
fi
}

main() {
  log INFO "[*] Initializing swarm cluster ..."

  if ! check_variables; then
    log ERROR "[x] Missing required environment variables."
    exit 1
  fi

  if ! create_env_file; then
    log ERROR "[x] Failed to create environment file."
    exit 1
  fi

  if ! copy_config_files; then
    log ERROR "[x] Failed to copy configuration files to remote server."
    exit 1
  fi

  if ! execute_initialization; then
    log ERROR "[x] Remote initialization failed."
    exit 1
  fi

  log INFO "[+] Initializing swarm cluster ...DONE"
}

main