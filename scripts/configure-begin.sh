#!/bin/bash

set -euo pipefail
source /tmp/variables.env
source /tmp/secrets.env

# Function to create a path if it does not already exist
createpath() {
  local newpath="$1"

  if [ ! -d "$newpath" ]; then
    echo "[*] Creating directory: $newpath"
    if ! mkdir -p "$newpath"; then
      echo "[x] Error: Failed to create directory '$newpath'"
      return 1
    fi
  fi

  echo "[*] Setting permissions on $newpath"
  sudo chmod -R 755 "$newpath"

  return 0
}

# Function to check if a Docker secret is in use
issecretinuse() {
  local secret_name="$1"

  echo "[*] Checking if secret '$secret_name' is in use..."

  # Check services using the secret
  if docker service ls --format '{{.Name}}' | \
     xargs -I {} docker service inspect {} --format '{{range .Spec.TaskTemplate.ContainerSpec.Secrets}}{{if eq .SecretName "'"$secret_name"'"}}{{.SecretName}}{{end}}{{end}}' | \
     grep -q "$secret_name"; then
    return 0
  fi

  # Check running containers using the secret
  if docker ps --format '{{.Names}}' | \
     xargs -I {} docker inspect {} --format '{{range .Mounts}}{{if eq .Type "secret"}}{{.Name}}{{end}}{{end}}' | \
     grep -q "$secret_name"; then
    return 0
  fi

  return 1
}

# Function to create or update a Docker secret
createdockersecret() {
  local label="$1"
  local name="$2"
  local value="$3"

  echo "[*] Processing secret: $label"

  if docker secret inspect "$name" &>/dev/null; then
    echo "[*] Secret '$name' already exists."

    if issecretinuse "$name"; then
      echo "[*] Secret '$name' is in use. Skipping deletion."
      return 0
    fi

    echo "[*] Removing old secret '$name'..."
    docker secret rm "$name"
  fi

  if [ -n "$name" ]; then
    echo "$value" | docker secret create "$name" -
    echo "[*] Secret '$name' created."
  else
    echo "[!] Secret name is not defined for $label."
  fi
}

# Function to create a Docker overlay network if it doesn't exist
createnetwork() {
  local network="$1"
  echo "[*] Ensuring Docker network '$network' exists..."

  docker network inspect "$network" --format '{{.Id}}' &>/dev/null || \
  docker network create --driver overlay "$network"
}

# Function that loads secrets as environment variables
loaddockersecrets() {
  # --- Load /tmp/secrets.env ---
  echo "[*] Loading secrets from /tmp/secrets.env..."

  while IFS='=' read -r key value || [ -n "$key" ]; do
    # Skip blank lines or comments
    [[ -z "$key" || "$key" == \#* ]] && continue

    # Remove surrounding quotes from value if any
    value="${value%\"}"
    value="${value#\"}"

    createdockersecret "$key" "$key" "$value"
  done < /tmp/secrets.env
}

# Create default nodelables
createnodelabels() {
  # Get the current hostname
  hostname=$(hostname)
  # Extract the role from hostname (3rd part in hyphen-separated string)
  srvrole=$(echo "$hostname" | cut -d'-' -f3)
  echo "[*] Detected role: $srvrole"
  # Apply the label only if this is the manager node
  if [[ "$hostname" == *"manager-1"* ]]; then
    echo "[*] Applying role label to all nodes..."
    # Get all node hostnames
    for node in $(docker node ls --format '{{.Hostname}}'); do
      # Extract the role from each node's hostname
      role=$(echo "$node" | cut -d'-' -f3)
      echo "    - Setting role=$role on $node"
      docker node update --label-add role=$role "$node"
    done
  fi
}

main() {
  local hostname
  hostname=$(hostname)
  echo "[*] Configuring Swarn Node: $hostname..."
  
  # Create the necessary directories
  createpath "/opt/app"

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
  echo "[*] Configuring Swarn Node: $hostname...DONE"
}

main
