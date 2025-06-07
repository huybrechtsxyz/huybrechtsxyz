#!/bin/bash

# FUNCTION: Cleanup orphaned VXLAN interfaces
# This function checks for orphaned VXLAN interfaces and deletes them if found.
# Removes orphaned VXLAN interfaces that cause Docker Swarm deployment errors.
# This is a workaround for the issue where Docker Swarm fails to deploy due to orphaned VXLAN interfaces.
# Problem this solves:
# Docker Swarm deployment may fail with an error like: network sandbox join failed: subnet
cleanup_vxlan_interfaces() {
  log INFO "    - Checking for orphaned VXLAN interfaces..."
  interfaces=$(ls /sys/class/net | grep '^vx-')

  if [ -z "$interfaces" ]; then
    log INFO "    - No orphaned VXLAN interfaces found."
    return
  fi

  for iface in $interfaces; do
    log INFO "    - Found orphaned interface: $iface"
    ip -d link show "$iface"
    log INFO "    - Deleting interface $iface..."
    sudo ip link delete "$iface"
  done

  log INFO "    - Checking for orphaned VXLAN interfaces...DONE"
}

# Create the paths defined in the metadata.json of the service/conf
createpaths_from_metadata() {
  local service="$1"
  local metadata_file="$APP_PATH/$service/conf/service.json"

  if [[ ! -f "$metadata_file" ]]; then
    log ERROR "[!] Metadata file '$metadata_file' not found."
    return 1
  fi

  local paths
  paths=$(jq -c '.service.paths[]?' "$metadata_file")

  if [[ -z "$paths" ]]; then
    log WARN "[!] No servicepaths defined for service '$service'"
    return 0
  fi

  while IFS= read -r path_entry; do
    local subpath chmod dirpath
    subpath=$(echo "$path_entry" | jq -r '.path')
    chmod=$(echo "$path_entry" | jq -r '.chmod')
    dirpath="$APP_PATH/$service/$subpath"

    if [[ ! -d "$dirpath" ]]; then
      log INFO "    - Creating directory: $dirpath"
      if ! mkdir -p "$dirpath"; then
        log ERROR "[!] Failed to create directory '$dirpath'"
        continue
      fi
    fi

    log INFO "    -Setting permissions on $dirpath to $chmod"
    sudo chmod -R "$chmod" "$dirpath"
  done <<< "$paths"

  return 0
}

# Parse the command line options for start/stop
parse_options() {
  ENV_FILE=""
  GROUP=""
  SERVICES=()
  while [[ $# -gt 0 ]]; do
    case "$1" in
      -d)
        DEPLOY="$2"
        shift 2
        ;;
      -e)
        ENV_FILE="$2"
        shift 2
        ;;
      -g)
        GROUP="$2"
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
        echo "Usage: $0 [-d appstack] [-e envfile] [-r role] [-s service1 service2 ...]"
        return 1
        ;;
    esac
  done

  echo "[*] Starting environment with ..."
  echo "    - Environment file: $ENV_FILE"
  echo "    - Services: ${SERVICES[*]}"
  echo "    - Group: $GROUP"
  echo "    - Stack: $DEPLOY"
}

# Copy consul discovery files to consul/etc
copy_to_consul() {
  CONSUL_CONF_PATH="$APP_PATH/consul/etc"
  rm -rf "$CONSUL_CONF_PATH/*"
  # Loop through all service directories in /opt/app
  for dir in "$APP_PATH"/*/; do
    # Get the service name from the directory path
    service=$(basename "$dir")
    # Define expected consul file path
    consul_file="$dir/conf/service.json"
    # If the consul file exists, copy it
    if [[ -f "$consul_file" ]]; then
      log INFO "    - Found $consul_file, copying to $CONSUL_CONF_PATH"
      cp "$consul_file" "$CONSUL_CONF_PATH/service.$service.json"
    fi
  done
}

# FUNCTION: Clear logs for a service
function clear_logs() {
    local service_path="$1"
    local log_dir="${service_path}/logs"
    if [[ -d "$log_dir" ]]; then
        log INFO "    - Clearing logs for ${service_path}..."
        rm -rf "$log_dir"/*
    fi
}

# Logging function
log() {
  local level="$1"; shift
  local msg="$*"
  local timestamp
  timestamp=$(date +"%Y-%m-%d %H:%M:%S")
  # case "$level" in
  #   INFO)    echo -e "[\033[1;34mINFO\033[0m]  $timestamp - $msg" ;;
  #   WARN)    echo -e "[\033[1;33mWARN\033[0m]  $timestamp - $msg" ;;
  #   ERROR)   echo -e "[\033[1;31mERROR\033[0m] $timestamp - $msg" >&2 ;;
  #   *)       echo -e "[UNKNOWN] $timestamp - $msg" ;;
  # esac
  case "$level" in
    INFO)    echo -e "[\033[1;34mINFO\033[0m]  - $msg" ;;
    WARN)    echo -e "[\033[1;33mWARN\033[0m]  - $msg" ;;
    ERROR)   echo -e "[\033[1;31mERROR\033[0m] - $msg" >&2 ;;
    *)       echo -e "[UNKNOWN] - $msg" ;;
  esac
}