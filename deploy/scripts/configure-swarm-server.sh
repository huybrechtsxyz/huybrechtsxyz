#!/bin/bash
set -euo pipefail
REMOTE_IP="$1"

source "$(dirname "${BASH_SOURCE[0]}")/../../scripts/functions.sh"

create_secret_file() {
  generate_env_file "APP_" "./src/pipeline.env"
  generate_env_file "SECRET_" "./src/secrets.env"
}

init_copy_files() {
log INFO "[*] Initializing REMOTE configuration..."
if ! ssh -o StrictHostKeyChecking=no root@"$REMOTE_IP" << EOF
mkdir -p "$APP_PATH_TEMP" "$APP_PATH_TEMP"/deploy "$APP_PATH_TEMP"/src
echo "[*] Initializing REMOTE server...DONE"
EOF
then
log ERROR "[!] Initializing configuration failed on $REMOTE_IP"
exit 1
fi
}

copy_config_files() {
  log INFO "[*] Copying environment files to remote server..."
  shopt -s nullglob
  log INFO "[*] Copying environment files to remote server...Deploy"
  scp -o StrictHostKeyChecking=no \
    ./deploy/scripts/*.sh \
    root@"$REMOTE_IP":"$APP_PATH_TEMP"/deploy || {
      log ERROR "[x] Failed to transfer configuration scripts to remote server"
      exit 1
    }
  log INFO "[*] Copying environment files to remote server...Sources"
  scp -o StrictHostKeyChecking=no \
    ./deploy/*.* \
    ./scripts/*.sh \
    ./src/*.* \
    root@"$REMOTE_IP":"$APP_PATH_TEMP"/src || {
      log ERROR "[x] Failed to transfer configuration scripts to remote server"
      exit 1
    }
  log INFO "[+] Copying environment files to remote server...DONE"
}

copy_service_files() {
  log INFO "[*] Copying service configuration files to remote server..."
  for service in ./src/*; do
    if [ -d "$service" ]; then
      service_name=$(basename "$service")
      remote_conf_dir="$APP_PATH_TEMP/src/$service_name"
      log INFO "[*] Copying service configuration files to remote server...$service_name"
      ssh -o StrictHostKeyChecking=no root@"$REMOTE_IP" "mkdir -p '$remote_conf_dir' && rm -f '$remote_conf_dir'/*.*" || {
        log ERROR "[!] Failed to create or cleanup remote config directory for $service_name"
        exit 1
      }
      scp -o StrictHostKeyChecking=no "$service"/*.* root@"$REMOTE_IP":"$remote_conf_dir/" || {
        log ERROR "[!] Failed to transfer $service_name configuration to remote server"
        exit 1
      }
    fi
  done
  log INFO "[+] Copying service configuration files to remote server...DONE"
}

configure_server() {
log INFO "[*] Executing REMOTE configuration..."
if ! ssh -o StrictHostKeyChecking=no root@"$REMOTE_IP" << EOF
set -e
echo "[*] Executing on REMOTE server..."
echo "[*] Using temporary path: $APP_PATH_TEMP"
shopt -s nullglob
echo "[*] Executing on REMOTE server...Copy source files"
set -a
source "$APP_PATH_TEMP/src/pipeline.env"
source "$APP_PATH_TEMP/src/$APP_ENVIRONMENT.env"
source "$APP_PATH_TEMP/src/secrets.env"
source "$APP_PATH_TEMP/src/functions.sh"
set +a
chmod +x "$APP_PATH_TEMP/deploy/configure-remote-server.sh"
"$APP_PATH_TEMP/deploy/configure-remote-server.sh"
rm -f "$APP_PATH_CONF"/secrets.env
echo "[*] Executing on REMOTE server...DONE"
EOF
then
log ERROR "[!] Remote configuration failed on $REMOTE_IP"
exit 1
fi
}

main() {
  log INFO "[*] Configuring remote server at $REMOTE_IP..."

  if ! create_secret_file; then
    log ERROR "[x] Failed to create environment file."
    exit 1
  fi

  if ! init_copy_files; then
    log ERROR "[x] Failed to initialize remote configuration"
    exit 1
  fi

  if ! copy_config_files; then
    log ERROR "[x] Failed to copy configuration files to remote server"
    exit 1
  fi

  if ! copy_service_files; then
    log ERROR "[x] Failed to copy service configuration files to remote server"
    exit 1
  fi

  if ! configure_server; then
    log ERROR "[x] Failed to configure remote server"
    exit 1
  fi

  log INFO "[+] Configuring remote server at $REMOTE_IP...DONE"
}

main
