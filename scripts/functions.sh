#!/bin/bash

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

# Create the paths defined in the metadata.json of the service/conf
createpaths_from_metadata() {
  local service="$1"
  local metadata_file="$APP_PATH/$service/conf/metadata.json"

  if [[ ! -f "$metadata_file" ]]; then
    echo "[x] Error: Metadata file '$metadata_file' not found."
    return 1
  fi

  local paths
  paths=$(jq -c '.servicepaths[]?' "$metadata_file")

  if [[ -z "$paths" ]]; then
    echo "[*] No servicepaths defined for service '$service'"
    return 0
  fi

  while IFS= read -r path_entry; do
    local subpath chmod dirpath
    subpath=$(echo "$path_entry" | jq -r '.path')
    chmod=$(echo "$path_entry" | jq -r '.chmod')
    dirpath="$APP_PATH/$service/$subpath"

    if [[ ! -d "$dirpath" ]]; then
      echo "[*] Creating directory: $dirpath"
      if ! mkdir -p "$dirpath"; then
        echo "[x] Error: Failed to create directory '$dirpath'"
        continue
      fi
    fi

    echo "[*] Setting permissions on $dirpath to $chmod"
    sudo chmod -R "$chmod" "$dirpath"
  done <<< "$paths"

  return 0
}

# Parse the command line options for start/stop
parse_options() {
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
        return 1
        ;;
    esac
  done

  echo "[*] Stopping services..."
  echo "Role: $ROLE"
  echo "Services: ${SERVICES[*]}"
}

# Copy consul discovery files to consul/conf
copy_to_consul() {
  CONSUL_CONF_PATH="$APP_PATH/consul/conf"
  # Loop through all service directories in /opt/app
  for dir in "$APP_PATH"/*/; do
    # Get the service name from the directory path
    service=$(basename "$dir")
    # Define expected consul file path
    consul_file="$dir/conf/consul.${service}.json"
    # If the consul file exists, copy it
    if [[ -f "$consul_file" ]]; then
      echo "[*] Found $consul_file, copying to $CONSUL_CONF_PATH"
      cp "$consul_file" "$CONSUL_CONF_PATH/"
    fi
  done
}

# FUNCTION: Clear logs for a service
function clear_logs() {
    local service_path="$1"
    local log_dir="${service_path}/logs"
    if [[ -d "$log_dir" ]]; then
        echo "Clearing logs for ${service_path}..."
        rm -rf "$log_dir"/*
        echo "Logs cleared."
    fi
}
