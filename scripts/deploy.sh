#!/bin/bash

set -euo pipefail

# Initialize script
SCRIPT_PATH="$(cd "$(dirname "$(readlink -f "${BASH_SOURCE[0]}")")" && pwd)"
source "$SCRIPT_PATH/functions.sh"
parse_options "$@"
load_envfile "vars-$ENV_FILE.env"

log INFO "[*] Starting services..."
parse_options "$@"
cd "$APP_PATH_CONF" || exit 1

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

# Clean VXLAN interfaces
log INFO "[*] Cleaning up VXLAN interfaces..."
cleanup_vxlan_interfaces
log INFO "[+] Cleaning up VXLAN interfaces... DONE"

# Prepare service selection list
SELECTION=()

# Make sure consul config if exists
consul_target="$APP_PATH_CONF/consul/etc"
if [[ ! -d "$consul_target" ]]; then
  mkdir -p $consul_target
  chmod 755 -R $consul_target
  if [[ -f "$APP_PATH_CONF/consul/config.json" ]]; then
    log INFO "[+] Moved Consul config to $consul_config_target"
    mv -f "$APP_PATH_CONF/consul/config.json" "$consul_target/consul.json"
  else
    log WARN "[!] consul/config.json not found in $APP_PATH_CONF. Skipping Consul configuration."
  fi
fi

# Loop each service
for dir in "$APP_PATH_CONF"/*/; do
  service_dir="${dir%/}"  # Remove trailing slash
  service_name=$(basename "$service_dir")
  service_file="$service_dir/service.json"

  log INFO "[*] Configuring service: $service_name"

  if [[ ! -f "$service_file" ]]; then
    log WARN "[!] $service_file not found. Skipping..."
    continue
  fi

  consul_conf="$service_dir/consul.json"
  if [[ -f "$consul_conf" ]]; then
    cp -f "$consul_conf" "$consul_target/consul.$service_name.json"
  fi

  # Load and parse service.json
  service_data=$(< "$service_file")
  service_id=$(echo "$service_data" | jq -r '.service.id')
  service_group=$(echo "$service_data" | jq -r '.service.groups[]?')
  service_endpoint=$(echo "$service_data" | jq -r '.service.endpoint')
  service_priority=$(echo "$service_data" | jq -r '.service.priority')

  # Iterate over each path entry
  service_paths=$(echo "$service_data" | jq -c '.service.paths[]?')
  for entry in $service_paths; do
    # Extract fields
    entry_path=$(echo "$entry" | jq -r '.path')
    entry_type=$(echo "$entry" | jq -r '.type')

    # Map type to base path
    case "$entry_type" in
      config) base_path="$APP_PATH_CONF" ;;
      data)   base_path="$APP_PATH_DATA" ;;
      logs)   base_path="$APP_PATH_LOGS" ;;
      serve)  base_path="$APP_PATH_SERV" ;;
      *)      echo "Unknown type: $entry_type" >&2; continue ;;
    esac

    # Build full target path
    if [ -n "$entry_path" ]; then
      target_path="$base_path/$service_name/$entry_path"
    else
      target_path="$base_path/$service_name"
    fi

    # Clear logs if it's a logs path
    if [[ "$entry_type" == "logs" ]]; then
      log INFO "[*] Clearing logs in $target_path"
      rm -rf "$target_path"/*
    fi
  done

  # Expand env variables in endpoint
  service_endpoint=$(expand_env_vars "$service_endpoint")

  # Match based on group or service list
  if [[ "$GROUP" == "$service_group" || " ${SERVICES[*]} " == *" $service_id "* || ( -z "$GROUP" && "${#SERVICES[@]}" -eq 0 ) ]]; then
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
  service_path="$APP_PATH_CONF/$id"
  compose_file="$service_path/compose.yml"

  if [[ ! -f "$compose_file" ]]; then
    log ERROR "[!] Compose file not found for $id. Skipping deployment."
    continue
  fi

  log INFO "[*] Deploying stack for $id (priority $priority)"
  docker stack deploy -c "$compose_file" "$STACK" --with-registry-auth --detach
done

# Cleanup
log INFO "[*] Removing temporary environment file..."
rm -f "$APP_PATH_CONF/.env"

log INFO "[+] All services started successfully."
log INFO "[+] Listing deployed services..."
docker service ls
