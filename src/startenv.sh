#!/bin/bash

# FUNCTION: Clear logs for each service
function clear_logs() {
    local service_name="$1"
    local log_dir="$base_dir/$service_name/logs"
    if [[ -d "$log_dir" ]]; then
        echo "Clearing logs for $service_name..."
        rm -rf "$log_dir"/*
        echo "Logs cleared for $service_name."
    fi
}

# START ENVIRONMENT
echo "Starting environment..."
base_dir="/app"
environment_file="/app/.env"
compose_file="/app/compose.yml"

# Clear logs for "consul" "traefik" "minio" "postgres" "keycloak" "telemetry"
echo "Clearing log directories..."
services=("traefik")
for service in "${services[@]}"; do
    clear_logs "$service"
done

echo "Loading environment variables..."
if [[ -f "$environment_file" ]]; then
    export $(grep -v '^#' "$environment_file" | xargs)
fi

echo "Validating Docker Compose..."
docker compose -f "$compose_file" --env-file "$environment_file" config || exit 1

echo "Starting Docker Swarm..."
docker stack deploy -c "$compose_file" app --detach=true

echo "Starting Docker Swarm...starting"
docker service ls
