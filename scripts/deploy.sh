#!/bin/bash
set -euo pipefail

APP_PATH="/opt/app"
source "$APP_PATH/functions.sh"

log INFO "[*] Starting services..."
parse_options "$@"
cd "$APP_PATH" || exit 1

# Validate ENV_FILE input
if [[ -z "${ENV_FILE:-}" ]]; then
  log ERROR "[*] No -e specified. Environment file is required."
  exit 1
fi

# Default stack name if not set
STACK="${STACK:-app}"

# Validate service selection input
if [[ -z "${GROUP:-}" && ${#SERVICES[@]} -eq 0 ]]; then
  log ERROR "[!] You must specify at least -g <group> or -s <service1 service2 ...>"
  log ERROR "Usage: $0 [-e envfile] [-g group] [-s service1 service2 ...]"
  exit 1
fi

# Count Docker manager and infra nodes
export DOCKER_MANAGERS
DOCKER_MANAGERS=$(docker node ls --filter "role=manager" --format '{{.Hostname}}' | wc -l)
log INFO "[*] Number of Docker manager nodes: $DOCKER_MANAGERS"

export DOCKER_INFRAS
DOCKER_INFRAS=$(docker node ls --filter "node.label=infra=true" --format '{{.Hostname}}' | wc -l)
log INFO "[*] Number of Docker infra nodes: $DOCKER_INFRAS"

export DOCKER_WORKERS
DOCKER_WORKERS=$(docker node ls --filter "node.label=worker=true" --format '{{.Hostname}}' | wc -l)
log INFO "[*] Number of Docker worker nodes: $DOCKER_WORKERS"

# Load environment variables
HOSTNAMEID=$(hostname)
ENV_PATH="$APP_PATH/$ENV_FILE.env"

if [[ -f "$ENV_PATH" ]]; then
  export $(grep -v '^#' "$ENV_PATH" | xargs)
  log INFO "[*] Loaded environment variables from $ENV_PATH"
  cp -f "$ENV_PATH" "$APP_PATH/.env"
  log INFO "[*] Copied environment variables to $APP_PATH/.env"
else
  log ERROR "[!] Environment file $ENV_PATH not found."
  exit 1
fi

# Clean VXLAN interfaces
log INFO "[*] Cleaning up VXLAN interfaces..."
cleanup_vxlan_interfaces
log INFO "[+] Cleaning up VXLAN interfaces... DONE"

# Prepare service selection list
SELECTION=()

for dir in "$APP_PATH"/*/; do
  service_dir="${dir%/}"  # Remove trailing slash
  service_name=$(basename "$service_dir")
  service_file="$service_dir/service.json"

  log INFO "[*] Configuring service: $service_name"

  if [[ ! -f "$service_file" ]]; then
    log WARN "[!] $service_file not found. Skipping..."
    continue
  fi

  # Copy Consul config if exists
  consul_conf="$service_dir/conf/consul.json"
  if [[ -f "$consul_conf" ]]; then
    cp -f "$consul_conf" "$APP_PATH/consul/etc/consul.$service_name.json"
  fi

  # Load and parse service.json
  service_data=$(< "$service_file")
  service_paths=$(echo "$service_data" | jq -r '.service.paths[]?.path')

  for entry_path in $service_paths; do
    target_path="$service_dir/$entry_path"
    chmod_value=$(echo "$service_data" | jq -r --arg p "$entry_path" '.service.paths[] | select(.path == $p) | .chmod')

    [[ -d "$target_path" ]] || mkdir -p "$target_path"
    chmod "$chmod_value" "$target_path"

    if [[ "$entry_path" == "logs" ]]; then
      log INFO "[*] Clearing logs in $target_path"
      rm -rf "$target_path"/*
    fi
  done

  service_id=$(echo "$service_data" | jq -r '.service.id')
  service_group=$(echo "$service_data" | jq -r '.service.groups[]?')
  service_endpoint=$(echo "$service_data" | jq -r '.service.endpoint')
  service_priority=$(echo "$service_data" | jq -r '.service.priority')

  # Expand env variables in endpoint
  while [[ "$service_endpoint" =~ \${([^}]+)} ]]; do
      var_name="${BASH_REMATCH[1]}"
      var_value="${!var_name:-}"
      service_endpoint="${service_endpoint//\${$var_name}/$var_value}"
  done

  # Match based on group or service list
  if [[ "$GROUP" == "$service_group" || " ${SERVICES[*]} " == *" $service_id "* || ( -z "${GROUP:-}" && ${#SERVICES[@]:-0} -eq 0 ) ]]; then
      SELECTION+=("$service_id|$service_priority|$service_endpoint")
      log INFO "[+] Service $service_name SELECTED"
  fi

  log INFO "[+] Configuration complete for $service_name"
done

# Sort selected services by priority
IFS=$'\n' sorted_services=($(printf "%s\n" "${SELECTION[@]}" | sort -t'|' -k2))

# Deploy selected services
for svc in "${sorted_services[@]}"; do
  IFS='|' read -r id priority endpoint <<< "$svc"
  service_path="$APP_PATH/$id"
  compose_file="$service_path/conf/compose.yml"

  if [[ ! -f "$compose_file" ]]; then
    log ERROR "[!] Compose file not found for $id. Skipping deployment."
    continue
  fi

  log INFO "[*] Deploying stack for $id (priority $priority)"
  docker stack deploy -c "$compose_file" "$STACK" --with-registry-auth --detach
done

# Cleanup
log INFO "[*] Removing temporary environment file..."
rm -f "$APP_PATH/.env"

log INFO "[+] All services started successfully."
log INFO "[+] Listing deployed services..."
docker service ls
