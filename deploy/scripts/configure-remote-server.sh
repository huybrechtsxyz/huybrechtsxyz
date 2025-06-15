#!/bin/bash
set -euo pipefail

# source /tmp/app/.env (set in pipeline)

createnetwork() {
  local network="$1"
  echo "[*] Ensuring Docker network '$network' exists..."

  if docker network inspect "$network" --format '{{.Id}}' &>/dev/null; then
    echo "[=] Docker network '$network' already exists."
  else
    echo "[+] Creating Docker overlay network '$network'..."
    if docker network create --driver overlay "$network"; then
      echo "[+] Docker network '$network' created successfully."
    else
      echo "[x] Failed to create Docker network '$network'. Is Docker Swarm mode enabled?"
      return 1
    fi
  fi
}

# Function that loads secrets as Docker secrets from a .env-style file
loaddockersecrets() {
  local secrets_file="/tmp/app/secrets.env"

  echo "[*] Loading secrets from $secrets_file..."

  if [[ ! -f "$secrets_file" ]]; then
    echo "[x] Secrets file not found: $secrets_file"
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

    echo "[=] Creating Docker secret: $key"
    createdockersecret "$key" "$key" "$value"
  done < "$secrets_file"

  echo "[+] Finished loading secrets."
}

# Function to create or update a Docker secret
createdockersecret() {
  local label="$1"
  local name="$2"
  local value="$3"

  echo "[*] Processing secret: $label"

  if [[ -z "$name" ]]; then
    echo "[!] Secret name is not defined for $label. Skipping."
    return 1
  fi

  if [[ -z "$value" ]]; then
    echo "[!] Secret value for '$name' is empty. Skipping."
    return 0
  fi

  if docker secret inspect "$name" &>/dev/null; then
    echo "[*] Secret '$name' already exists."

    if issecretinuse "$name"; then
      echo "[*] Secret '$name' is in use. Skipping deletion and creation."
      return 0
    fi

    echo "[*] Removing old secret '$name'..."
    docker secret rm "$name"
  fi

  # Use printf to avoid trailing newline
  if printf "%s" "$value" | docker secret create "$name" -; then
    echo "[+] Secret '$name' created."
  else
    echo "[x] Failed to create secret '$name'."
    return 1
  fi
}

# Function to check if a Docker secret is in use
issecretinuse() {
  local secret_name="$1"

  echo "[*] Checking if secret '$secret_name' is in use..."

  # Check services using the secret
  if docker service ls --format '{{.Name}}' | \
     xargs -r -n1 -I{} docker service inspect {} --format '{{range .Spec.TaskTemplate.ContainerSpec.Secrets}}{{if eq .SecretName "'"$secret_name"'"}}1{{end}}{{end}}' 2>/dev/null | \
     grep -q "1"; then
    echo "[*] Secret '$secret_name' is in use by a Docker service."
    return 0
  fi

  # Check containers using the secret (Swarm services mount secrets as /run/secrets/*)
  if docker ps --format '{{.ID}}' | \
     xargs -r -n1 -I{} docker inspect {} --format '{{range .Mounts}}{{if and (eq .Type "bind") (hasPrefix .Source "/var/lib/docker/swarm/secrets/")}}{{.Name}}{{end}}{{end}}' 2>/dev/null | \
     grep -q "$secret_name"; then
    echo "[*] Secret '$secret_name' is in use by a running container."
    return 0
  fi

  echo "[*] Secret '$secret_name' is not in use."
  return 1
}

createnodelabels() {
  echo "[*] Applying role label to all nodes..."
  # Get the current hostname
  local hostname
  hostname=$(hostname)

  # Extract the role from hostname (3rd part in hyphen-separated string)
  local srvrole
  srvrole=$(echo "$hostname" | cut -d'-' -f3)
  echo "[*] ... Detected role: $srvrole"

  # Get the workspace information from the environment variable
  echo "[*] ... Getting workspace information"
  local workspace_file="$APP_PATH_TEMP/workspace.$WORKSPACE.json"

  # Fetch all node hostnames once
  echo "[*] ... Getting node hostnames"
  local nodes
  mapfile -t nodes < <(docker node ls --format '{{.Hostname}}')

  for node in "${nodes[@]}"; do
    echo "[*] ... Applying role label to $node..."
    local role instance server
    role=$(echo "$node" | cut -d'-' -f3)
    instance=$(echo "$node" | cut -d'-' -f4)
    server="${role}-${instance}"
    echo "[*] ... Setting $role=true on $node"
    echo "[*] ... Setting role=$role on $node"
    echo "[*] ... Setting server=$server on $node"
    echo "[*] ... Setting instance=$instance on $node"

    # Inspect current labels
    echo "[*] ... Inspecting labels on $node"
    while IFS='=' read -r k v; do
      if [[ "$k" =~ ^[a-zA-Z0-9_.-]+$ && -n "$v" ]]; then
        existing_labels["$k"]="$v"
      else
        echo "[!] Skipping malformed label: $k=$v"
      fi
    done < <(docker node inspect "$node" --format '{{range $k, $v := .Spec.Labels}}{{printf "%s=%s\n" $k $v}}{{end}}')

    # Standard labels
    echo "[*] ... Declaring labels and values on $node"
    : "${role:?role is unset}"
    : "${server:?server is unset}"
    : "${instance:?instance is unset}"
    declare -A desired_labels
    desired_labels["$role"]="true"
    desired_labels["role"]="$role"
    desired_labels["server"]="$server"
    desired_labels["instance"]="$instance"
    
    # Add standard labels if needed
    for key in "${!desired_labels[@]}"; do
      if [[ "${existing_labels[$key]}" != "${desired_labels[$key]}" ]]; then
        echo "[*] ... Setting $key=${desired_labels[$key]}"
        docker node update --label-add "$key=${desired_labels[$key]}" "$node" || echo "[!] Warning: Failed to set $key"
      fi
    done

    # Add custom labels from JSON if needed
    mapfile -t ws_labels < <(jq -r --arg id "$node" '.servers[] | select(.id == $id) | .labels[]?' "$workspace_file")
    for label in "${ws_labels[@]}"; do
      key="${label%%=*}"
      val="${label#*=}"
      if [[ "${existing_labels[$key]}" != "$val" ]]; then
        echo "[*] ... Adding custom label $label"
        docker node update --label-add "$label" "$node" || echo "[!] Warning: Failed to add $label"
      fi
      desired_labels["$key"]="$val"  # Mark as desired to avoid removal
    done

    # Delete undesired labels at the end
    echo "[*] ... Cleaning up obsolete labels..."
    for key in "${!existing_labels[@]}"; do
      if [[ -z "${desired_labels[$key]}" ]]; then
        echo "[*]         - Removing $key"
        docker node update --label-rm "$key" "$node" || echo "[!] Warning: Failed to remove $key"
      fi
    done

    unset existing_labels
    unset desired_labels

  done

  echo "[+] Applying role label to all nodes...DONE"
  return 0
}

main() {
  local hostname
  hostname=$(hostname)
  echo "[*] Configuring Swarn Node: $hostname..."

  docker info --format '{{.Swarm.LocalNodeState}}' | grep -q "active" || {
    echo "[x] Docker Swarm is not active. Run 'docker swarm init' first."
    exit 1
  }

  # Create docker networks and secrets only leader node
  if [[ "$hostname" == *"manager-1"* ]]; then
    createnetwork "wan-$WORKSPACE"
    createnetwork "lan-$WORKSPACE"
    createnetwork "lan-test"
    createnetwork "lan-staging"
    createnetwork "lan-production"
    loaddockersecrets
    createnodelabels
  fi

}

main