#!/bin/bash
set -euo pipefail

# Initialize script
SCRIPT_PATH="$(cd "$(dirname "$(readlink -f "${BASH_SOURCE[0]}")")" && pwd)"
source "$SCRIPT_PATH/functions.sh"
log INFO "[*] Starting environment..."
parse_options "$@"

# Validate ENV_FILE input
if [[ -z "${ENV_FILE:-}" ]]; then
  log ERROR "[*] No -e specified. Environment file is required."
  exit 1
fi

# Load the environment file
# Save all the variables in the .env file
load_envfile "$SCRIPT_PATH/pipeline.env"
load_envfile "$SCRIPT_PATH/variables.env"
load_envfile "$SCRIPT_PATH/$ENV_FILE.env"
generate_env_file "" "$SCRIPT_PATH/.env"
cd "$PATH_CONF" || exit 1

# Default stack name if not set
STACK="${STACK:-app}"

# Validate service selection input
if [[ -z "${GROUP:-}" && ${#SERVICES[@]} -eq 0 ]]; then
  log ERROR "[!] You must specify at least -g <group> or -s <service1 service2 ...>"
  log ERROR "Usage: $0 [-e envfile] [-g group] [-s service1 service2 ...]"
  exit 1
fi

# Count Docker manager and infra nodes
update_docker_variables "DOCKER_MANAGERS" "role=manager" "$PATH_CONF/.env" "Docker Manager Nodes"
update_docker_variables "DOCKER_INFRAS" "node.label=infra=true" "$PATH_CONF/.env" "Docker Infra Nodes"
update_docker_variables "DOCKER_WORKERS" "node.label=worker=true" "$PATH_CONF/.env" "Docker Worker Nodes"

# Clean VXLAN interfaces
log INFO "[*] Cleaning up VXLAN interfaces..."
cleanup_vxlan_interfaces
log INFO "[+] Cleaning up VXLAN interfaces... DONE"

# Prepare service selection list
SELECTION=()

# Make sure consul config if exists
consul_target="$PATH_CONF/consul/etc"
if [[ ! -d "$consul_target" ]]; then
  mkdir -p $consul_target
  chmod 755 -R $consul_target
  if [[ -f "$PATH_CONF/consul/config.json" ]]; then
    log INFO "[+] Moved Consul config to $consul_target"
    mv -f "$PATH_CONF/consul/config.json" "$consul_target/consul.json"
  else
    log WARN "[!] consul/config.json not found in $PATH_CONF. Skipping Consul configuration."
  fi
fi

# Loop each service
for dir in "$PATH_CONF"/*/; do
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
#log INFO "[*] Removing temporary environment file..."
#rm -f "$APP_PATH_CONF/.env"

log INFO "[+] All services started successfully."
log INFO "[+] Listing deployed services..."
docker service ls
