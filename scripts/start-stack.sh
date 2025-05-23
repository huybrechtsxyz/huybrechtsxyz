#!/bin/bash

# FUNCTION: Clear logs for each service
function clear_logs() {
    local service_path="$1"
    local log_dir="${service_path}/logs"
    if [[ -d "$log_dir" ]]; then
        echo "Clearing logs for ${service_path}..."
        rm -rf "$log_dir"/*
        echo "Logs cleared."
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
  interfaces=$(ls /sys/class/net | grep '^vx-')

  if [ -z "$interfaces" ]; then
    echo "✅ No orphaned VXLAN interfaces found."
    return
  fi

  for iface in $interfaces; do
    echo "Found orphaned interface: $iface"
    ip -d link show "$iface"
    echo "Deleting interface $iface..."
    sudo ip link delete "$iface"
  done

  echo "VXLAN cleanup completed."
}

# Start
echo "Starting environment..."

# Parse
ROLE=""
SERVICES=()
while [[ $# -gt 0 ]]; do
  case "$1" in
    -r)
      ROLE="$2"
      shift 2
      ;;
    -s)
      shift
      while [[ $# -gt 0 && "$1" != -* ]]; do
        SERVICES+=("$1")
        shift
      done
      ;;
    *)
      echo "Usage: $0 [-r role] [-s service1 service2 ...]"
      exit 1
      ;;
  esac
done

# Get the basic directory and environment file
echo "[*] Starting services..."
cd "$APP_PATH" || exit 1

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

# Process services
for dir in ./src/*/; do
  METADATA_FILE="${dir}metadata.json"
  COMPOSE_FILE="${dir}compose.yml"
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



# Loading environment variables
echo "Loading environment variables..."
environment_file="/opt/app/.env"
export HOSTNAMEID=$(hostname)
if [[ -f "$environment_file" ]]; then
    export $(grep -v '^#' "$environment_file" | xargs)
fi

# Count the number of manager nodes
DOCKER_MANAGERS=$(docker node ls --filter "role=manager" --format '{{.Hostname}}' | wc -l)
export DOCKER_MANAGERS=$DOCKER_MANAGERS
echo "Number of Docker manager nodes: $DOCKER_MANAGERS"

# Cleanup of interfaces
cleanup_vxlan_interface

# Set permissions for specific directories
echo "Setting permissions for directories..."
chmod -R 755 /opt/app
chmod -R 600 /opt/app/traefik/data
chmod -R 777 /opt/app/traefik/logs
chmod -R 777 /opt/app/consul/data
chmod -R 777 /opt/app/postgres/data
chmod -R 777 /opt/app/postgres/admin
chmod -R 777 /opt/app/postgres/backups

# Check services
for dir in ./src/*/; do
  METADATA_FILE="${dir}metadata.json"
  COMPOSE_FILE="${dir}compose.yml"
  LOG_PATH="${dir}logs/"

  if [ ! -f "$METADATA_FILE" ] || [ ! -f "$COMPOSE_FILE" ]; then
    continue
  fi

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
    if [[ "$MATCHED" == false ]]; then
      continue
    fi
  fi

  # Clear logs
  clear_logs "$META_SERVICE"

  # Start Docker Swarm if not already running
  echo "Validating Docker Compose..."
  docker compose -f "$COMPOSE_FILE" --env-file "$environment_file" config > ${dir}/logs/compose.log || exit 1
  echo "Starting Docker Swarm..."
  docker stack deploy -c "$COMPOSE_FILE" $META_SERVICE --detach=true

done

echo "Starting Docker Swarm...starting"
docker service ls
