#!/bin/bash
set -euo pipefail
REMOTE_IP="$1"

load_variables() {
  log INFO "[*] Loading variables from ./src/$ENVIRONMENT.env file..."
  while IFS='=' read -r key value || [ -n "$key" ]; do
    [[ -z "$key" || "$key" == \#* ]] && continue
    value="${value%\"}"
    value="${value#\"}"
    echo "$key=$value" >> "$GITHUB_ENV"
  done < ./src/$ENVIRONMENT.env
  log INFO "[+] Loading variables from ./src/$ENVIRONMENT.env file...DONE"
}

load_secrets() {
log INFO "[*] Loading secrets for remote environment..."
cat <<EOF > ./src/secrets.env
PLATFORM_USERNAME=${PLATFORM_USERNAME}
PLATFORM_PASSWORD=${PLATFORM_PASSWORD}
TRAEFIK_CLIENTID=${TRAEFIK_CLIENTID}
TRAEFIK_SECRET=${TRAEFIK_SECRET}
VERSIO_USERNAME=${VERSIO_USERNAME}
VERSIO_PASSWORD=${VERSIO_PASSWORD}
EOF
log INFO "[+] Copying secrets for remote environment...DONE"
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
shopt -s nullglob
echo "[*] Executing on REMOTE server...Copy source files"
cp -f "$APP_PATH_TEMP"/src/*.* "$APP_PATH_CONF"
chmod +x "$APP_PATH_CONF"/*.sh
set -a
source "$APP_PATH_CONF/$ENVIRONMENT.env"
source "$APP_PATH_CONF/secrets.env"
source "$APP_PATH_CONF/function.sh"
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

  if ! load_variables; then
    log ERROR "[x] Failed to load environment variables from ./src/$ENVIRONMENT.env"
    exit 1
  fi

  if ! load_secrets; then
    log ERROR "[x] Failed to load secrets for remote environment"
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
