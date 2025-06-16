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

    # Map type to base path
    case "$entry_type" in
      config) base_path="$APP_PATH_CONF" ;;
      data)   base_path="$APP_PATH_DATA" ;;
      logs)   base_path="$APP_PATH_LOGS" ;;
      serve)  base_path="$APP_PATH_SERV" ;;
      *)      log ERROR "[!] Unknown type: $entry_type" >&2; continue ;;
    esac

    # Build full target path
    if [ -n "$entry_path" ]; then
      target_path="$base_path/$service/$entry_path"
    else
      target_path="$base_path/$service"
    fi

    # Create and chmod the path
    [[ -d "$target_path" ]] || mkdir -p "$target_path"
    chmod "$chmod_value" "$target_path"
  done

  return 0
}