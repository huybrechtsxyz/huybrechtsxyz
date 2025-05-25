#!/bin/bash

log INFO "[*] Starting services..."

source /opt/app/.env
source /opt/app/functions.sh

# Get the basic directory and environment file
cd "$APP_PATH" || exit 1
parse_options "$@"

# Load environment variables
export HOSTNAMEID=$(hostname)
environment_file="/opt/app/.env"
if [[ -f "$environment_file" ]]; then
    export $(grep -v '^#' "$environment_file" | xargs)
    log INFO "[*] Loaded environment variables from $environment_file"
else
    log WARN "[!] Environment file $environment_file not found"
fi

# Count manager nodes
export DOCKER_MANAGERS=$(docker node ls --filter "role=manager" --format '{{.Hostname}}' | wc -l)
echo "[*] Number of Docker manager nodes: $DOCKER_MANAGERS"

log INFO "[*] Cleaning up VXLAN interfaces..."
cleanup_vxlan_interfaces
log INFO "[*] Cleaning up VXLAN interfaces...DONE"

log INFO "[*] Setting permissions on relevant directories..."
chmod -R 755 /opt/app
chmod -R 600 /opt/app/traefik/data
chmod -R 777 /opt/app/traefik/logs
chmod -R 777 /opt/app/consul/data
chmod -R 777 /opt/app/postgres/data
chmod -R 777 /opt/app/postgres/admin
chmod -R 777 /opt/app/postgres/backups
log INFO "[*] Setting permissions on relevant directories...DONE"

log INFO "[*] Copying Consul discovery files..."
copy_to_consul
log INFO "[*] Copying Consul discovery files...DONE"

# Process services
for dir in /opt/app/*/; do
  METADATA_FILE="${dir}conf/metadata.json"
  COMPOSE_FILE="${dir}conf/compose.yml"
  LOG_PATH="${dir}logs/"

  if [[ ! -f "$METADATA_FILE" || ! -f "$COMPOSE_FILE" ]]; then
    log WARN "[!] Missing metadata or compose file in $dir. Skipping..."
    continue
  fi

  META_GROUP=$(jq -r .group "$METADATA_FILE")
  META_SERVICE=$(jq -r .service "$METADATA_FILE")

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
  docker stack deploy -c "$COMPOSE_FILE" "$META_SERVICE" --detach=true
done

log INFO "[+] Starting services...DONE"
log INFO "[+] Listing services..."
docker service ls
