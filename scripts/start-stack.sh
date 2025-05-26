#!/bin/bash

log INFO "[*] Starting services..."

source /opt/app/functions.sh

# Get the basic directory and environment file
APP_PATH="/opt/app"
parse_options "$@"
cd "$APP_PATH" || exit 1

if [[ -z "$ENV_FILE" ]]; then
    ENV_FILE="shared"
    log ERROR "[*] No -e specified."
    exit 1
fi

# Validate required input
if [[ -z "$GROUP" && ${#SERVICES[@]} -eq 0 ]]; then
  log ERROR "[!] You must specify at least -g <group> or -s <service1 service2 ...>"
  log ERROR "Usage: $0 [-e envfile] [-g group] [-s service1 service2 ...]"
  return 1
fi

# Load environment variables
export HOSTNAMEID=$(hostname)
environment_file="$APP_PATH/$ENV_FILE.env"
if [[ -f "$environment_file" ]]; then
    export $(grep -v '^#' "$environment_file" | xargs)
    log INFO "[*] Loaded environment variables from $environment_file"
    cp -f "$environment_file" /opt/app/.env
    log INFO "[*] Copied environment variables to /opt/app/.env"
else
    log WARN "[!] Environment file $environment_file not found"
    exit 1
fi

# Count manager nodes
export DOCKER_MANAGERS=$(docker node ls --filter "role=manager" --format '{{.Hostname}}' | wc -l)
echo "[*] Number of Docker manager nodes: $DOCKER_MANAGERS"

log INFO "[*] Cleaning up VXLAN interfaces..."
cleanup_vxlan_interfaces
log INFO "[*] Cleaning up VXLAN interfaces...DONE"

log INFO "[*] Setting permissions on relevant directories..."
chmod -R 755 $APP_PATH
chmod -R 600 $APP_PATH/traefik/data
chmod -R 777 $APP_PATH/traefik/logs
chmod -R 777 $APP_PATH/consul/data
chmod -R 777 $APP_PATH/postgres/data
chmod -R 777 $APP_PATH/postgres/admin
chmod -R 777 $APP_PATH/postgres/backups
log INFO "[*] Setting permissions on relevant directories...DONE"

log INFO "[*] Copying Consul discovery files..."
copy_to_consul
log INFO "[*] Copying Consul discovery files...DONE"

# Process services
for dir in $APP_PATH/*/; do
  METADATA_FILE="${dir}conf/metadata.json"
  COMPOSE_FILE="${dir}conf/compose.yml"
  LOG_PATH="${dir}logs/"

  if [[ ! -f "$METADATA_FILE" || ! -f "$COMPOSE_FILE" ]]; then
    log WARN "[!] Missing metadata or compose file in $dir. Skipping..."
    continue
  fi

  META_GROUP=$(jq -r .group "$METADATA_FILE")
  META_SERVICE=$(jq -r .service "$METADATA_FILE")
  META_STACK=$(jq -r .name "$METADATA_FILE")

  if [[ -n "$GROUP" && "$GROUP" != "$META_GROUP" ]]; then
    log INFO "[-] Skipping $META_SERVICE (group mismatch: $META_GROUP)"
    continue
  fi

  if [[ ${#SERVICES[@]} -gt 0 ]]; then
    MATCHED=false
    for svc in "${SERVICES[@]}"; do
      if [[ "$svc" == "$META_SERVICE" ]]; then
        MATCHED=true
        break
      fi
    done
    if [[ "$MATCHED" == false ]]; then
      log INFO "[-] Skipping $META_SERVICE (not in selected service list)"
      continue
    fi
  fi

  mkdir -p "${LOG_PATH}"
  clear_logs "$dir"

  log INFO "[*] Validating Docker Compose for $META_SERVICE..."
  if ! docker compose -f "$COMPOSE_FILE" --env-file "$environment_file" config > "${LOG_PATH}/compose.log"; then
    log ERROR "[!] Docker Compose validation failed for $META_SERVICE. Check ${LOG_PATH}/compose.log"
    exit 1
  fi

  log INFO "[+] Deploying $META_SERVICE to Docker Swarm..."
  docker stack deploy -c "$COMPOSE_FILE" "$META_STACK" --detach=true
done

log INFO "[*] Cleaning up temporary /opt/app/.env"
rm -f /opt/app/.env

log INFO "[+] Starting services...DONE"
log INFO "[+] Listing services..."
docker service ls
