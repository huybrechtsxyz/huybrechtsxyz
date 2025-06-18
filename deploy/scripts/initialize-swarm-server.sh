#!/bin/bash
set -euo pipefail

if [ "$#" -ne 3 ]; then
  log ERROR "Usage: $0 <APP_REMOTE_IP> <APP_PRIVATE_IP> <APP_MANAGER_IP>"
  exit 1
fi

APP_REMOTE_IP="$1"
APP_PRIVATE_IP="$2"
APP_MANAGER_IP="$3"

generate_env_file() {
  log INFO "[*] Generating environment file with all APP_ variables..."

  # Find all environment variable names starting with APP_
  mapfile -t app_vars < <(compgen -v | grep '^APP_')

  # Check that each APP_ var is set and non-empty
  for var in "${app_vars[@]}"; do
    [[ -z "${!var}" ]] && { echo "[!] Error: Missing required variable '$var'" >&2; exit 1; }
  done

  # Generate the env file with all APP_ vars
  {
    echo "# Auto-generated environment file (all APP_ variables)"
    for var in "${app_vars[@]}"; do
      # Escape values to avoid issues (optional)
      printf 'export %s=%q\n' "$var" "${!var}"
    done
  } > ./deploy/scripts/initialize.env

  log INFO "[+] Generating environment file with all APP_ variables...DONE"
}

copy_config_files() {
log INFO "[*] Copying initialization script to remote server..."
ssh -o StrictHostKeyChecking=no root@$APP_REMOTE_IP << EOF
  mkdir -p "$APP_PATH_TEMP"
  chmod 777 "$APP_PATH_TEMP"
EOF

log INFO "[*] Copying initialization scripts and cluster config to remote server..."
scp -o StrictHostKeyChecking=no \
  ./deploy/scripts/* \
  ./deploy/*.* \
  ./scripts/functions.sh \
  root@"$APP_REMOTE_IP":"$APP_PATH_TEMP"/ || {
    echo "[x] Failed to transfer initialization scripts to remote server"
    exit 1
  }
}

execute_initialization() {
log INFO "[*] Executing REMOTE initialization..."
if ! ssh -o StrictHostKeyChecking=no root@"$APP_REMOTE_IP" << EOF
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
  log ERROR "[!] Remote initialization failed on $APP_REMOTE_IP"
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