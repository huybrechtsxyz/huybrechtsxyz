#!/bin/bash
set -euo pipefail
REMOTE_IP="$1"

load_variables() {
  echo "[*] Loading variables from ./src/$ENVIRONMENT.env file..."
  while IFS='=' read -r key value || [ -n "$key" ]; do
    [[ -z "$key" || "$key" == \#* ]] && continue
    value="${value%\"}"
    value="${value#\"}"
    echo "$key=$value" >> "$GITHUB_ENV"
  done < ./src/$ENVIRONMENT.env
  echo "[*] Loading variables from ./src/$ENVIRONMENT.env file...DONE"
}

load_secrets() {
echo "[*] Loading secrets for remote environment..."
cat <<EOF > ./deploy/scripts/secrets.env
PLATFORM_USERNAME=${PLATFORM_USERNAME}
PLATFORM_PASSWORD=${PLATFORM_PASSWORD}
TRAEFIK_CLIENTID=${TRAEFIK_CLIENTID}
TRAEFIK_SECRET=${TRAEFIK_SECRET}
VERSIO_USERNAME=${VERSIO_USERNAME}
VERSIO_PASSWORD=${VERSIO_PASSWORD}
EOF
echo "[+] Copying secrets for remote environment...DONE"
}

copy_config_files() {
  echo "[*] Copying environment files to remote server..."
  scp -o StrictHostKeyChecking=no \
    ./deploy/scripts/* \
    ./deploy/cluster.$WORKSPACE.json \
    ./src/*.* \
    root@"$REMOTE_IP":"$APP_PATH_TEMP"/ || {
      echo "[x] Failed to transfer configuration scripts to remote server"
      exit 1
    }
  echo "[+] Copying environment files to remote server...DONE"
}

copy_service_files() {
  echo "[*] Copying service configuration files to remote server..."
  for service in ./src/*; do
    if [ -d "$service" ]; then
      service_name=$(basename "$service")
      remote_conf_dir="$APP_PATH_CONF/$service_name"
      echo "[*] Copying service configuration files to remote server...$service_name"
      ssh -o StrictHostKeyChecking=no root@"$REMOTE_IP" "mkdir -p '$remote_conf_dir' && rm -f '$remote_conf_dir'/*" || {
        echo "[x] Failed to create or cleanup remote config directory for $service_name"
        exit 1
      }
      scp -o StrictHostKeyChecking=no "$service"/*.* root@"$REMOTE_IP":"$remote_conf_dir/" || {
        echo "[x] Failed to transfer $service_name configuration to remote server"
        exit 1
      }
    fi
  done
  echo "[+] Copying service configuration files to remote server...DONE"
}

configure_server() {
echo "[*] Executing REMOTE configuration..."
if ! ssh -o StrictHostKeyChecking=no root@"$REMOTE_IP" << EOF
set -e
echo "[*] Executing on REMOTE server..."
set -a
source "$APP_PATH_TEMP/$ENVIRONMENT.env"
source "$APP_PATH_TEMP/secrets.env"
set +a
chmod +x "$APP_PATH_TEMP/configure-remote-server.sh"
"$APP_PATH_TEMP/configure-remote-server.sh"
echo "[*] Executing on REMOTE server...DONE"
EOF
then
echo "[!] Remote configuration failed on $REMOTE_IP"
exit 1
fi
}

main() {
  echo "[*] Configuring remote server at $REMOTE_IP..."

  load_variables || exit 1
  load_secrets || exit 1
  copy_config_files || exit 1
  copy_service_files || exit 1
  configure_server || exit 1

  echo "[+] Configuration complete."
}

main
