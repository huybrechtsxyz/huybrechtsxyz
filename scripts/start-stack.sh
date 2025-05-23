#!/bin/bash

echo "[*] Starting services..."

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
fi

# Count manager nodes
export DOCKER_MANAGERS=$(docker node ls --filter "role=manager" --format '{{.Hostname}}' | wc -l)
echo "Number of Docker manager nodes: $DOCKER_MANAGERS"

# VXLAN Cleanup
cleanup_vxlan_interfaces

# Set permissions (optional refinement here)
chmod -R 755 /opt/app
chmod -R 600 /opt/app/traefik/data
chmod -R 777 /opt/app/traefik/logs
chmod -R 777 /opt/app/consul/data
chmod -R 777 /opt/app/postgres/data
chmod -R 777 /opt/app/postgres/admin
chmod -R 777 /opt/app/postgres/backups

# Copy consul discovery files to consul/conf
copy_to_consul

# Process services
for dir in ./src/*/; do
  METADATA_FILE="${dir}conf/metadata.json"
  COMPOSE_FILE="${dir}conf/compose.yml"
  LOG_PATH="${dir}logs/"

  [[ -f "$METADATA_FILE" && -f "$COMPOSE_FILE" ]] || continue

  META_ROLE=$(jq -r .role "$METADATA_FILE")
  META_SERVICE=$(jq -r .service "$METADATA_FILE")

  if [[ -n "$ROLE" && "$ROLE" != "$META_ROLE" ]]; then
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
    [[ "$MATCHED" == false ]] && continue
  fi

  mkdir -p "${LOG_PATH}"
  clear_logs "$dir"

  echo "Validating Docker Compose for $META_SERVICE..."
  docker compose -f "$COMPOSE_FILE" --env-file "$environment_file" config > "${LOG_PATH}/compose.log" || exit 1

  echo "Deploying $META_SERVICE to Docker Swarm..."
  docker stack deploy -c "$COMPOSE_FILE" "$META_SERVICE" --detach=true
done

echo "Environment started. Listing active Docker services:"
docker service ls