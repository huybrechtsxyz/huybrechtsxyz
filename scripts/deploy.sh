#!/bin/bash
set -euo pipefail

# -------------------------------------------------------------------
# Initialize script paths and source dependencies
# -------------------------------------------------------------------
SCRIPT_PATH="$(cd "$(dirname "$(readlink -f "${BASH_SOURCE[0]}")")" && pwd)"
source "$SCRIPT_PATH/utilities.sh"

log INFO "[*] Starting deployment process..."
parse_options "$@"

# -------------------------------------------------------------------
# Validate environment selection
# -------------------------------------------------------------------
if [[ -z "${ENV_FILE:-}" ]]; then
  log ERROR "[!] No environment (-e) specified. Exiting."
  exit 1
fi

# -------------------------------------------------------------------
# Load and merge environment configurations
# -------------------------------------------------------------------
load_envfile "$SCRIPT_PATH/services.env"
load_envfile "$SCRIPT_PATH/$ENV_FILE.env"

# Prioritize ENV_FILE.env values by reversing the order in awk
cat "$SCRIPT_PATH/$ENV_FILE.env" "$SCRIPT_PATH/services.env" | awk -F= '!seen[$1]++' > "$SCRIPT_PATH/.env"

log INFO "[*] Combined environment written to .env"
cd "$SCRIPT_PATH" || exit 1

STACK="${STACK:-app}"  # Default stack name

# -------------------------------------------------------------------
# Load and merge environment configurations
# While waiting for pull request #3104 to be merged
# https://github.com/oauth2-proxy/oauth2-proxy/pull/3104/files
# -------------------------------------------------------------------
COOKIE_FILE="$SCRIPT_PATH/cookie.env"
COOKIE_NAME="OAUTH2_TRAEFIK_COOKIE"
if [ ! -f "$COOKIE_FILE" ]; then
  touch "$COOKIE_FILE"
fi
# Check if the variable is present and has a value
EXISTING_VALUE=$(grep -E "^${COOKIE_NAME}=" "$COOKIE_FILE" | cut -d'=' -f2-)
if [ -z "$EXISTING_VALUE" ]; then
  # Generate a 32-byte random base64 secret
  NEW_SECRET=$(openssl rand -base64 32)
  # If variable already exists as an empty value, replace it
  if grep -q "^${COOKIE_NAME}=" "$COOKIE_FILE"; then
    sed -i "s|^${COOKIE_NAME}=.*|${COOKIE_NAME}=${NEW_SECRET}|" "$COOKIE_FILE"
  else
    # Otherwise, append it
    echo "${COOKIE_NAME}=${NEW_SECRET}" >> "$COOKIE_FILE"
  fi
  EXISTING_VALUE=$NEW_SECRET
  log INFO "[+] Generated new cookie secret and updated .env"
else
  log INFO "[+] Existing cookie secret found in .env (nothing changed)"
fi
load_envfile "$COOKIE_FILE"
# Update or append in env
if grep -q "^${COOKIE_NAME}=" "$SCRIPT_PATH/.env"; then
  sed -i "s|^${COOKIE_NAME}=.*|${COOKIE_NAME}=${EXISTING_VALUE}|" "$SCRIPT_PATH/.env"
else
  echo "${COOKIE_NAME}=${EXISTING_VALUE}" >> "$SCRIPT_PATH/.env"
fi

# -------------------------------------------------------------------
# Validate input options
# -------------------------------------------------------------------
if [[ -z "${GROUP:-}" && ${#SERVICES[@]} -eq 0 ]]; then
  log ERROR "[!] You must specify either -g <group> or -s <service1 service2 ...>"
  log ERROR "Usage: $0 [-e envfile] [-g group] [-s service1 service2 ...]"
  exit 1
fi

# -------------------------------------------------------------------
# Populate Docker node variables
# -------------------------------------------------------------------
update_docker_variables "DOCKER_MANAGERS" "role=manager" "$SCRIPT_PATH/.env" "Docker Manager Nodes"
update_docker_variables "DOCKER_INFRAS" "node.label=infra=true" "$SCRIPT_PATH/.env" "Docker Infra Nodes"
update_docker_variables "DOCKER_WORKERS" "node.label=worker=true" "$SCRIPT_PATH/.env" "Docker Worker Nodes"

# -------------------------------------------------------------------
# Clean up existing VXLAN interfaces
# -------------------------------------------------------------------
log INFO "[*] Cleaning up VXLAN interfaces..."
cleanup_vxlan_interfaces
log INFO "[+] VXLAN cleanup complete."

# -------------------------------------------------------------------
# Process service selection
# -------------------------------------------------------------------
SELECTION=()

for dir in "$SCRIPT_PATH"/*/; do
  service_dir="${dir%/}"
  service_file="$service_dir/config/service.json"

  if [[ ! -f "$service_file" ]]; then
    log WARN "[!] $service_file not found. Skipping..."
    continue
  fi

  service_data=$(< "$service_file")
  service_id=$(echo "$service_data" | jq -r '.service.id')
  service_group=$(echo "$service_data" | jq -r '.service.groups[]?' || true)
  service_endpoint=$(echo "$service_data" | jq -r '.service.endpoint // empty')
  service_priority=$(echo "$service_data" | jq -r '.service.priority // 100')

  if [[ "$service_id" == "consul" ]]; then
    log INFO "[+] Moving Consul configuration to /consul/etc"
    if [ -e "$SCRIPT_PATH/consul/config/config.json" ]; then
      if [ -e "$SCRIPT_PATH/consul/etc/config.json" ]; then
          mv -f "$SCRIPT_PATH/consul/config/config.json" "$SCRIPT_PATH/consul/etc/config.json"
      else
          echo "Error: Destination file does not exist: $SCRIPT_PATH/consul/etc/config.json" >&2
          exit 1
      fi
    fi
  fi



  if [[ "$GROUP" == "$service_group" || " ${SERVICES[*]} " == *" $service_id "* || ( -z "$GROUP" && "${#SERVICES[@]}" -eq 0 ) ]]; then
    SELECTION+=("$service_id|$service_priority|$service_endpoint")
    log INFO "[+] Selected service: $service_id (priority $service_priority)"

    consul_conf="$service_dir/config/consul.json"
    if [[ -f "$consul_conf" ]]; then
      cp -f "$consul_conf" "$SCRIPT_PATH/consul/etc/consul.$service_id.json"
    fi

    log_dir="$service_dir/logs"
    if [[ -d "$log_dir" ]]; then
      log INFO "[*] Clearing logs in $log_dir"
      rm -rf "${log_dir:?}"/*
    fi
  fi

  log INFO "[+] Processed service: $service_id"
done

# -------------------------------------------------------------------
# Deploy services in priority order
# -------------------------------------------------------------------
IFS=$'\n' sorted_services=($(printf "%s\n" "${SELECTION[@]}" | sort -t'|' -k2,2n))

for svc in "${sorted_services[@]}"; do
  IFS='|' read -r id priority endpoint <<< "$svc"
  compose_file="$SCRIPT_PATH/$id/config/compose.yml"

  if [[ ! -f "$compose_file" ]]; then
    log ERROR "[!] Compose file not found for $id. Skipping deployment."
    continue
  fi

  log INFO "[*] Deploying $id with priority $priority..."
  docker stack deploy -c "$compose_file" "$STACK" --with-registry-auth --detach >> "$SCRIPT_PATH"/stack.log 2>&1
  log INFO "[+] Deployed $id"
done

# -------------------------------------------------------------------
# Post-deployment summary
# -------------------------------------------------------------------
log INFO "[+] All selected services deployed successfully."
log INFO "[+] Currently running Docker services:"
docker service ls
