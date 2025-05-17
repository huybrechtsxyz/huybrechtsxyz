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

# FUNCTION: Cleanup orphaned VXLAN interfaces
# This function checks for orphaned VXLAN interfaces and deletes them if found.
# Removes orphaned VXLAN interfaces that cause Docker Swarm deployment errors.
# This is a workaround for the issue where Docker Swarm fails to deploy due to orphaned VXLAN interfaces.
# Problem this solves:
# Docker Swarm deployment may fail with an error like: network sandbox join failed: subnet
cleanup_vxlan_interfaces() {
  echo "Checking for orphaned VXLAN interfaces..."

  # Find all VXLAN interfaces
  interfaces=$(ls /sys/class/net | grep '^vx-')

  if [ -z "$interfaces" ]; then
    echo "✅ No orphaned VXLAN interfaces found."
    return
  fi

  # Loop through and delete each orphaned VXLAN interface
  for iface in $interfaces; do
    echo "Found orphaned interface: $iface"
    echo "Details:"
    ip -d link show "$iface"
    echo "Deleting interface $iface..."
    sudo ip link delete "$iface"
  done

  echo "VXLAN cleanup completed."
}

# START ENVIRONMENT
echo "Starting environment..."
export HOSTNAMEID=$(hostname)

# Call the cleanup function
cleanup_vxlan_interface

# Get the basic directory and environment file
cd /app
base_dir="/app"
environment_file="/app/.env"
compose_file="/app/compose.yml"
services=("traefik" "consul" "postgres" "pgadmin" "keycloak")

# Clear logs for "consul" "traefik" "minio" "postgres" "keycloak" "telemetry"
echo "Clearing log directories..."
rm -rf /app/config.log
for service in "${services[@]}"; do
    clear_logs "$service"
done

# Set permissions for specific directories
echo "Setting permissions for directories..."
chmod -R 755 /app
chmod -R 600 /app/traefik/data
chmod -R 777 /app/traefik/logs
chmod -R 777 /app/consul/data
chmod -R 777 /app/postgres/data
chmod -R 777 /app/postgres/admin
chmod -R 777 /app/postgres/backups

# Loading environment variables
echo "Loading environment variables..."
if [[ -f "$environment_file" ]]; then
    export $(grep -v '^#' "$environment_file" | xargs)
fi

# Count the number of manager nodes
DOCKER_MANAGERS=$(docker node ls --filter "role=manager" --format '{{.Hostname}}' | wc -l)
export DOCKER_MANAGERS=$DOCKER_MANAGERS
echo "Number of Docker manager nodes: $DOCKER_MANAGERS"

# Start Docker Swarm if not already running
echo "Validating Docker Compose..."
docker compose -f "$compose_file" --env-file "$environment_file" config > config.log || exit 1

echo "Starting Docker Swarm..."
docker stack deploy -c "$compose_file" app --detach=true

echo "Starting Docker Swarm...starting"
docker service ls
