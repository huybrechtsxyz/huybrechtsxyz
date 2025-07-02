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

# Load environment file if it exists
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

# Generate an environment file only taking env vars with specific prefix
generate_env_file() {
  local prefix="$1"
  local output_file="$2"

  if [[ -z "$prefix" || -z "$output_file" ]]; then
    echo "[!] Usage: generate_env_file <PREFIX> <OUTPUT_FILE>" >&2
    return 1
  fi

  log INFO "[*] Generating environment file for variables with prefix '$prefix'..."

  # Get all variables starting with the prefix
  mapfile -t vars < <(compgen -v | grep "^${prefix}")

  if [[ "${#vars[@]}" -eq 0 ]]; then
    echo "[!] Error: No environment variables found with prefix '$prefix'" >&2
    return 1
  fi

  # Validate all are non-empty
  for var in "${vars[@]}"; do
    [[ -z "${!var}" ]] && { echo "[!] Error: Missing required variable '$var'" >&2; return 1; }
  done

  # Ensure output directory exists
  mkdir -p "$(dirname "$output_file")"

  # Generate the env file with the prefix stripped
  {
    echo "# Auto-generated environment file (prefix '$prefix' stripped)"
    for var in "${vars[@]}"; do
      short_var="${var#$prefix}"
      printf '%s=%q\n' "$short_var" "${!var}"
    done
  } > "$output_file"

  log INFO "[+] Environment file generated at '$output_file'"
}

# Add specific docker variable to the .env file
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
