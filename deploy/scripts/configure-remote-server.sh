#!/bin/bash
set -eo pipefail

hostname=$(hostname)

createnetwork() {
  local network="$1"
  log INFO "[*] Ensuring Docker network '$network' exists..."

  if docker network inspect "$network" --format '{{.Id}}' &>/dev/null; then
    log INFO "[=] Docker network '$network' already exists."
  else
    log INFO "[+] Creating Docker overlay network '$network'..."
    if docker network create --driver overlay "$network"; then
      log INFO "[+] Docker network '$network' created successfully."
    else
      log ERROR "[x] Failed to create Docker network '$network'. Is Docker Swarm mode enabled?"
      return 1
    fi
  fi
}

issecretinuse() {
  local secret_name="$1"

  log INFO "[*] Checking if secret '$secret_name' is in use..."

  # Check services using the secret
  if docker service ls --format '{{.Name}}' | \
    xargs -r -n1 -I{} docker service inspect {} --format '{{range .Spec.TaskTemplate.ContainerSpec.Secrets}}{{if eq .SecretName "'"$secret_name"'"}}1{{end}}{{end}}' 2>/dev/null | \
    grep -q "1"; then
    log INFO "[*] Secret '$secret_name' is in use by a Docker service."
    return 0
  fi

  # Check containers using the secret (Swarm services mount secrets as /run/secrets/*)
  if docker ps --format '{{.ID}}' | \
    xargs -r -n1 -I{} docker inspect {} --format '{{range .Mounts}}{{if and (eq .Type "bind") (hasPrefix .Source "/var/lib/docker/swarm/secrets/")}}{{.Name}}{{end}}{{end}}' 2>/dev/null | \
    grep -q "$secret_name"; then
    log INFO "[*] Secret '$secret_name' is in use by a running container."
    return 0
  fi

  log INFO "[*] Secret '$secret_name' is not in use."
  return 1
}

createdockersecret() {
  local label="$1"
  local name="$2"
  local value="$3"

  log INFO "[*] Processing secret: $label"

  if [[ -z "$name" ]]; then
    log WARN "[!] Secret name is not defined for $label. Skipping."
    return 1
  fi

  if [[ -z "$value" ]]; then
    log WARN "[!] Secret value for '$name' is empty. Skipping."
    return 0
  fi

  if docker secret inspect "$name" &>/dev/null; then
    log INFO "[*] Secret '$name' already exists."

    if issecretinuse "$name"; then
      log INFO "[*] Secret '$name' is in use. Skipping deletion and creation."
      return 0
    fi

    log INFO "[*] Removing old secret '$name'..."
    docker secret rm "$name"
  fi

  # Use printf to avoid trailing newline
  if printf "%s" "$value" | docker secret create "$name" -; then
    log INFO "[+] Secret '$name' created."
  else
    log ERROR "[x] Failed to create secret '$name'."
    return 1
  fi
}

loaddockersecrets() {
  
  local secrets_file="$PATH_TEMP/src/secrets.env"

  log INFO "[*] Loading secrets from $secrets_file..."

  if [[ ! -f "$secrets_file" ]]; then
    log ERROR "[x] Secrets file not found: $secrets_file"
    return 1
  fi

  while IFS='=' read -r key value || [[ -n "$key" ]]; do
    # Skip blank lines or comments
    [[ -z "$key" || "$key" =~ ^\s*# ]] && continue

    # Trim whitespace
    key=$(echo "$key" | xargs)
    value=$(echo "$value" | xargs)

    # Remove surrounding quotes from value
    value="${value%\"}"
    value="${value#\"}"

    log INFO "[=] Creating Docker secret: $key"
    createdockersecret "$key" "$key" "$value"
  done < "$secrets_file"

  echo "[+] Finished loading secrets."
}

createnodelabels() {
  log INFO "[*] Applying role label to all nodes..."
  # Get the current hostname
  local hostname srvrole workspace_file nodes
  hostname=$(hostname)

  # Get current hostname and parse role (3rd part of hyphen-separated hostname)
  hostname=$(hostname)
  srvrole=$(echo "$hostname" | cut -d'-' -f3)
  log INFO "[*] ... Detected role: $srvrole"

  # Workspace JSON file path (environment variables assumed set)
  workspace_file="$PATH_TEMP/src/workspace.$WORKSPACE.json"
  log INFO "[*] ... Using workspace file: $workspace_file"

  # Get list of all Docker Swarm node hostnames
  log INFO "[*] ... Getting node hostnames"
  mapfile -t nodes < <(docker node ls --format '{{.Hostname}}')
  log INFO "[*] ... Found ${#nodes[@]} nodes"

  for node in "${nodes[@]}"; do
    log INFO "[*] ... Applying role label to $node..."

    # Parse role, instance from node name (3rd and 4th hyphen-separated parts)
    local role instance server
    role=$(echo "$node" | cut -d'-' -f3)
    instance=$(echo "$node" | cut -d'-' -f4)
    server="${role}-${instance}"
    log INFO "[*] ... Setting $role=true on $node"
    log INFO "[*] ... Setting role=$role on $node"
    log INFO "[*] ... Setting server=$server on $node"
    log INFO "[*] ... Setting instance=$instance on $node"

    # Initialize associative arrays
    declare -A existing_labels
    declare -A desired_labels

    # Read existing labels into associative array
    log INFO "[*] ... Reading existing labels on $node"
    while IFS='=' read -r k v; do
      [[ -z "$k" && -z "$v" ]] && continue  # Skip empty lines or pure "="
      if [[ "$k" =~ ^[a-zA-Z0-9_.-]+$ && -n "$v" ]]; then
        existing_labels["$k"]="$v"
      else
        log WARN "[!] Skipping malformed label: $k=$v"
      fi
    done < <(
      docker node inspect "$node" \
        --format '{{range $k, $v := .Spec.Labels}}{{printf "%s=%s\n" $k $v}}{{end}}' \
        | grep -E '^[^=]+=[^=]+$'  # Optional extra guard
    )

    # Define desired standard labels
    desired_labels["$role"]="true"
    desired_labels["role"]="$role"
    desired_labels["server"]="$server"
    desired_labels["instance"]="$instance"

    # Update/add standard labels if they differ or are missing
    log INFO "[*] ... Update/add standard labels $node"
    for key in "${!desired_labels[@]}"; do
      if [[ "${existing_labels[$key]}" != "${desired_labels[$key]}" ]]; then
        log INFO "[*] ... Setting $key=${desired_labels[$key]}"
        docker node update --label-add "$key=${desired_labels[$key]}" "$node" || echo "[!] Warning: Failed to set $key on $node"
      fi
    done

    # Add custom labels from workspace JSON (jq filters by node id)
    log INFO "[*] ... Add custom labels from workspace on $node"
    mapfile -t ws_labels < <(jq -r --arg id "$node" '.servers[] | select(.id == $id) | .labels[]?' "$workspace_file")
    for label in "${ws_labels[@]}"; do
      # Split label key and value
      local key="${label%%=*}"
      local val="${label#*=}"

      # Add or update label if needed
      if [[ "${existing_labels[$key]}" != "$val" ]]; then
        log INFO "[*] ... Adding custom label $label"
        docker node update --label-add "$label" "$node" || echo "[!] Warning: Failed to add $label on $node"
      fi

      # Mark as desired to avoid removal
      desired_labels["$key"]="$val"
    done

    # Remove any labels that exist but are not desired
    log INFO "[*] ... Cleaning up obsolete labels on $node"
    for key in "${!existing_labels[@]}"; do
      if [[ -z "${desired_labels[$key]}" ]]; then
        log INFO "[*] ... Removing $key"
        docker node update --label-rm "$key" "$node" || echo "[!] Warning: Failed to remove $key on $node"
      fi
    done

    # Clean up arrays before next iteration
    unset existing_labels
    unset desired_labels
  done

  log INFO "[+] Applying role label to all nodes...DONE"
  return 0
}

create_workspace_paths() {
  log INFO "[*] Creating workspace directories on mounted disks..."

  : "${WORKSPACE:?Missing WORKSPACE}"
  local hostname
  hostname=$(hostname)
  local workspace_file="$PATH_TEMP/src/workspace.$WORKSPACE.json"

  if [[ ! -f "$workspace_file" ]]; then
    log ERROR "[!] Workspace file not found: $workspace_file"
    return 1
  fi

  local server_id
  server_id=$(jq -r '.servers[].id' "$workspace_file" | while read -r id; do
    [[ "$hostname" == *"$id"* ]] && echo "$id" && break
  done)

  if [[ -z "$server_id" ]]; then
    log ERROR "[!] No matching server ID found for hostname: $hostname"
    return 1
  fi

  local mounts_json
  mounts_json=$(jq -r --arg id "$server_id" '.servers[] | select(.id == $id) | .mounts[] | @base64' "$workspace_file")

  for mount in $mounts_json; do
    local type disk
    type=$(echo "$mount" | base64 --decode | jq -r '.type')
    disk=$(echo "$mount" | base64 --decode | jq -r '.disk')
    mountpoint="/mnt/data$disk"

    # Find the path for this type
    dir_path=$(jq -r --arg t "$type" '.paths[] | select(.type == $t) | .path' "$workspace_file")

    if [[ -z "$dir_path" || -z "$mountpoint" ]]; then
      log WARN "[!] Missing path or mountpoint for type=$type, disk=$disk"
      continue
    fi

    target="$mountpoint$dir_path"
    if [[ ! -d "$target" ]]; then
      log INFO "[+] Creating directory: $target"
      mkdir -p "$target"
    else
      log INFO "[+] Directory already exists: $target"
    fi
  done

  log INFO "[*] Bind mounting workspace directories to system paths..."
  type_list=$(jq -r '.paths[].type' "$workspace_file")
  for type in $type_list; do
    # Get the relative path for this type
    dir_path=$(jq -r --arg t "$type" '.paths[] | select(.type == $t) | .path' "$workspace_file")
    # Find the disk number for this type mount
    disk_num=$(jq -r --arg t "$type" '.servers[] | select(.id == "'$server_id'") | .mounts[] | select(.type == $t) | .disk' "$workspace_file")

    mountpoint="/mnt/data$disk_num"
    source_dir="$mountpoint$dir_path"
    target_dir="$dir_path"

    if mountpoint -q "$target_dir"; then
      log INFO "[+] $target_dir is already a mountpoint."
    else
      # Ensure target directory exists
      if [[ ! -d "$target_dir" ]]; then
        log INFO "[+] Creating target directory $target_dir"
        mkdir -p "$target_dir"
      fi

      log INFO "[+] Bind mounting $source_dir to $target_dir"
      mount --bind "$source_dir" "$target_dir"
    fi
  done
  log INFO "[+] Bind mounts completed."

  log INFO "[+] All workspace directories created."
}

create_service_paths() {
  log INFO "[*] Creating directories for all services..."

  : "${WORKSPACE:?Missing WORKSPACE}"
  local hostname
  hostname=$(hostname)
  local workspace_file="$PATH_TEMP/src/workspace.$WORKSPACE.json"

  if [[ ! -f "$workspace_file" ]]; then
    log ERROR "[!] Workspace file not found: $workspace_file"
    return 1
  fi

  local server_id
  server_id=$(jq -r '.servers[].id' "$workspace_file" | while read -r id; do
    [[ "$hostname" == *"$id"* ]] && echo "$id" && break
  done)

  if [[ -z "$server_id" ]]; then
    log ERROR "[!] No matching server ID found for hostname: $hostname"
    return 1
  fi

  # Build type → mount location map
  declare -A MOUNT_MAP
  while IFS="|" read -r type disk; do
    MOUNT_MAP["$type"]="/mnt/data$disk"
  done < <(jq -r --arg id "$server_id" '.servers[] | select(.id == $id) | .mounts[] | "\(.type)|\(.disk)"' "$workspace_file")

  # Build type → base path map
  declare -A TYPE_PATH_MAP
  while IFS="|" read -r type basepath; do
    TYPE_PATH_MAP["$type"]="$basepath"
  done < <(jq -r '.paths[] | "\(.type)|\(.path)"' "$workspace_file")

  # Truncate the services.env file
  : > "$PATH_TEMP/src/services.env"

  # Process each service
  for svc_file in "$PATH_TEMP"/*/service.json; do
    [[ -f "$svc_file" ]] || continue
    local svc_name=$(basename "$(dirname "$svc_file")")

    log INFO "[*] Processing service: $svc_name"

    jq -c '.service.paths[]' "$svc_file" | while read -r path_obj; do
      local type path chmod
      type=$(echo "$path_obj" | jq -r '.type')
      path=$(echo "$path_obj" | jq -r '.path // ""')
      chmod=$(echo "$path_obj" | jq -r '.chmod // empty')

      local mount_base="${MOUNT_MAP[$type]}"
      local path_base="${TYPE_PATH_MAP[$type]}"

      if [[ -z "$mount_base" || -z "$path_base" ]]; then
        log WARN "[!] Unknown path type '$type' for service $svc_name — skipping."
        continue
      fi

      local full_path="$mount_base$path_base/$svc_name"
      [[ -n "$path" ]] && full_path="$full_path/$path"

      mkdir -p "$full_path"

      if [[ -n "$chmod" ]]; then
        chmod "$chmod" "$full_path"
        log INFO "[+] Created $full_path with chmod $chmod"
      else
        log INFO "[+] Created $full_path (default permissions)"
      fi

      # Generate variable name
      var_base=$(echo "$svc_name" | tr '[:lower:]' '[:upper:]')
      var_type=$(echo "$type" | tr '[:lower:]' '[:upper:]')
      var_path=$(echo "$path" | tr '[:lower:]' '[:upper:]')
      if [[ -n "$var_path" ]]; then
        var_name="${var_base}_PATH_${var_path}"
      else
        var_name="${var_base}_PATH_${var_type}"
      fi

      # Export + write to services.env
      echo "export $var_name=\"$full_path\"" >> "$PATH_TEMP/src/services.env"
      export "$var_name=$full_path"

    done


  done

  log INFO "[✓] All service paths created."
}

configure_server() {
  log INFO "[*] Configuring server..."

  log INFO "[*] Installing configuration files..."
  cp -f "$PATH_TEMP"/src/*.* "$PATH_CONF/" || {
    log ERROR "[x] Failed to copy configuration file to $PATH_CONF"
    return 1
  }

  log INFO "[*] Installing services..."
  for svc_file in "$PATH_TEMP"/src/*/service.json; do
    local svc_name=$(basename "$(dirname "$svc_file")")
    log INFO "[*] Installing service...$svc_name"
    mkdir -p "$PATH_CONF/$svc_name"
    cp -fr "$PATH_TEMP"/src/"$svc_name"/* "$PATH_CONF/$svc_name/"
  done

  log INFO "[*] Configuring server...DONE"
}

configure_services() {
  log INFO "[*] Configuring application services..."
  for script in $PATH_CONF/*/configure.sh; do
    service=$(basename "$(dirname "$script")")
    log INFO "[*] Configuring service '$service'..."

    remote_conf_dir="$PATH_CONF/$service"
    remote_script="$remote_conf_dir/configure.sh"

    chmod +x "$remote_script"
    "$remote_script"

    log INFO "[+] Configured service '$service'"
  done
  log INFO "[+] Configuring application services...DONE"
}

main() {
  log INFO "[*] Configuring Swarm Node: $hostname..."

  docker info --format '{{.Swarm.LocalNodeState}}' | grep -q "active" || {
    log ERROR "[x] Docker Swarm is not active. Run 'docker swarm init' first."
    exit 1
  }

  # Create docker networks and secrets only leader node
  if [[ "$hostname" == *"manager-1"* ]]; then
    log INFO "[+] Creating Docker networks..."
    createnetwork "wan-$WORKSPACE" || exit 1
    createnetwork "lan-$WORKSPACE" || exit 1
    createnetwork "lan-test" || exit 1
    createnetwork "lan-staging" || exit 1
    createnetwork "lan-production" || exit 1

    log INFO "[+] Loading Docker secrets..."
    loaddockersecrets || exit 1

    log INFO "[+] Creating node labels..."
    createnodelabels || exit 1
  fi

  log INFO "[*] Creating workspace paths ..."
  create_workspace_paths || exit 1

  log INFO "[*] Creating service paths ..."
  create_service_paths || exit 1
  
  log INFO "[*] Creating service paths ...DONE"
  configure_server || exit 1

  log INFO "[*] Configure services ..."
  configure_services || exit 1

  log INFO "[*] Remote server cleanup..."
  chmod 755 "$PATH_CONF"/*
  rm -f "$PATH_CONF"/develop.env
  rm -f "$PATH_CONF"/secrets.env
  rm -rf "/tmp/app/"*

  log INFO "[+] Configuring Swarm Node: $hostname...DONE"
}

main
