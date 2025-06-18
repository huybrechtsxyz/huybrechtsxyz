#!/bin/bash

# Logging function
log() {
  local level="$1"; shift
  local msg="$*"
  local timestamp
  timestamp=$(date +"%Y-%m-%d %H:%M:%S")
  case "$level" in
    INFO)    echo -e "[\033[1;34mINFO\033[0m]  - $msg" ;;
    WARN)    echo -e "[\033[1;33mWARN\033[0m]  - $msg" ;;
    ERROR)   echo -e "[\033[1;31mERROR\033[0m] - $msg" >&2 ;;
    *)       echo -e "[UNKNOWN] - $msg" ;;
  esac
}

# Parse the command line options for start/stop
parse_options() {
  ENV_FILE=""
  STACK=""
  GROUP=""
  SERVICES=()
  while [[ $# -gt 0 ]]; do
    case "$1" in
      -d|--stack)
        STACK="$2"
        shift 2
        ;;
      -e|--environment)
        ENV_FILE="$2"
        shift 2
        ;;
      -g|--group)
        GROUP="$2"
        shift 2
        ;;
      -s|--services)
        shift
        while [[ $# -gt 0 && "$1" != -* ]]; do
          SERVICES+=("$1")
          shift
        done
        ;;
      *)
        echo "Usage: $0 [-d appstack] [-e envfile] [-r role] [-s service1 service2 ...]"
        return 1
        ;;
    esac
  done

  log INFO "[*] ... Environment file: $ENV_FILE"
  log INFO "[*] ... Services: ${SERVICES[*]}"
  log INFO "[*] ... Group: $GROUP"
  log INFO "[*] ... Stack: $STACK"
}

# Function to load environment variables from a given file
load_envfile() {
    local env_file="$1"

    if [ -f "$env_file" ]; then
        log INFO "[*] ... Loading environment from: $env_file"
        set -a                # Automatically export all variables
        source "$env_file"
        set +a
    else
        log ERROR "[!] Error: env file not found: $env_file" >&2
        return 1
    fi
}

# FUNCTION: Cleanup orphaned VXLAN interfaces
# This function checks for orphaned VXLAN interfaces and deletes them if found.
# Removes orphaned VXLAN interfaces that cause Docker Swarm deployment errors.
# This is a workaround for the issue where Docker Swarm fails to deploy due to orphaned VXLAN interfaces.
# Problem this solves:
# Docker Swarm deployment may fail with an error like: network sandbox join failed: subnet
cleanup_vxlan_interfaces() {
  log INFO "[*] ... Checking for orphaned VXLAN interfaces..."
  #interfaces=$(ls /sys/class/net | grep '^vx-')
  interfaces=$(find /sys/class/net -maxdepth 1 -type l -name 'vx-*' -printf '%f\n' 2>/dev/null || true)
  if [ -z "$interfaces" ]; then
    log INFO "[*] ... No orphaned VXLAN interfaces found."
    return 0
  fi

  for iface in $interfaces; do
    log INFO "[*] ...Found orphaned interface: $iface"
    ip -d link show "$iface"
    log INFO "[*] ...Deleting interface $iface..."
    sudo ip link delete "$iface"
  done

  log INFO "[*] ...Checking for orphaned VXLAN interfaces...DONE"
}

# FUNCTION: Expands the environment variables with their value
expand_env_vars() {
  local input="$1"
  local output="$input"
  local var_name var_value full_match

  # Loop while there are still ${VAR_NAME} patterns in the string
  while [[ "$output" =~ (\${([^}]+)}) ]]; do
    full_match="${BASH_REMATCH[1]}"
    var_name="${BASH_REMATCH[2]}"
    var_value="${!var_name:-}"

    # Optionally warn if variable is not set
    if [[ -z "${!var_name+x}" ]]; then
      echo "Warning: Environment variable '$var_name' is not set." >&2
    fi

    # Replace the placeholder with its value
    output="${output//$full_match/$var_value}"
  done

  echo "$output"
}

create_service_paths() {
  local service="$1"
  local service_file="$APP_PATH_CONF/$service/service.json"

  if [[ ! -f "$service_file" ]]; then
    log ERROR "[!] $service_file not found. Skipping..."
    return 1
  fi

  local service_data=$(< "$service_file")

  # Iterate over each path entry
  service_paths=$(echo "$service_data" | jq -c '.service.paths[]?')
  for entry in $service_paths; do
    # Extract fields
    entry_path=$(echo "$entry" | jq -r '.path')
    entry_type=$(echo "$entry" | jq -r '.type')
    chmod_value=$(echo "$entry" | jq -r '.chmod')
    entry_disk=$(echo "$entry" | jq -r '.disk // 0')

    # Build the path variable for this service path
    case "$entry_type" in
      config) base_var="APP_PATH_CONF" ;;   # /etc/app
      data)   base_var="APP_PATH_DATA" ;;   # /var/lib/data or /var/lib/data1
      logs)   base_var="APP_PATH_LOGS" ;;   # /var/lib/logs or /var/lib/logs1
      serve)  base_var="APP_PATH_SERV" ;;   # /srv or /srv1
      *) echo "Unknown type: $entry_type" >&2; continue ;;
    esac

    # Append disk suffix if entry_disk > 0
    if [ "$entry_disk" -gt 0 ]; then
      base_var="$base_var$entry_disk"
    fi

    # Get the actual base path from the environment variable
    # and build the target path
    target_path="${!base_var:-}/$service_name"
    if [ -n "$entry_path" ]; then target_path="$target_path/$service/$entry_path"; fi

     # Create and chmod the path
    [[ -d "$target_path" ]] || mkdir -p "$target_path"
    chmod "$chmod_value" "$target_path"
  done

  return 0
}

update_docker_variables() {
  local var_name="$1"
  local filter="$2"
  local env_file="${3:-$APP_PATH_CONF/.env}"
  local description="$4"
  local count

  count=$(docker node ls --filter "$filter" --format '{{.Hostname}}' | wc -l)
  export "$var_name"="$count"

  log INFO "[*] Number of Docker nodes matching '$description': $count for $filter"

  if grep -q "^${var_name}=" "$env_file" 2>/dev/null; then
    sed -i "s|^${var_name}=.*|${var_name}=${count}|" "$env_file"
  else
    echo "${var_name}=${count}" >> "$env_file"
  fi
}

# Function to convert size string like "20G" or "1024M" to GB integer (approx)
size_to_gb() {
  local size=$1
  if [[ "$size" =~ ^([0-9]+)([GMK])$ ]]; then
    local val=${BASH_REMATCH[1]}
    local unit=${BASH_REMATCH[2]}
    case $unit in
      G) echo "$val" ;;
      M) echo $((val / 1024)) ;;
      K) echo 0 ;;
      *) echo 0 ;;
    esac
  else
    echo 0
  fi
}
