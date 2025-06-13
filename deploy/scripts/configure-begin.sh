#!/bin/bash

set -euo pipefail
source /tmp/variables.env
source /tmp/secrets.env

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

  if [ -z "$value" ]; then
    echo "[!] Secret value for '$name' is empty. Skipping."
    return 0
  fi

  if [ -n "$name" ]; then
    #echo "$value" | docker secret create "$name" -
    #echo has tendency to add newline at the end
    printf "%s" "$value" | docker secret create "$name" -
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
  if [[ "$hostname" != *"manager-1"* ]]; then
    echo "[+] Not on management node. No labels assigned."
    return
  fi
  
  echo "[*] Applying role label to all nodes..."
  # Get all node hostnames
  # Hostnames are: srv-{workspace}-{role}-{instance}-{random})
  # Extract the role type instance from each node's hostname
  for node in $(docker node ls --format '{{.Hostname}}'); do
    role=$(echo "$node" | cut -d'-' -f3)
    instance=$(echo "$node" | cut -d'-' -f4)
    server="${role}-${instance}"
    echo "[*]     - Setting $role=true on $node"
    echo "[*]     - Setting role=$role on $node"
    echo "[*]     - Setting server=$server on $node"
    echo "[*]     - Setting instance=$instance on $node"
    docker node update \
      --label-add $role=true \
      --label-add role=$role \
      --label-add server=$server \
      --label-add instance=$instance \
      "$node"
  done

  echo "[*] Updating infra-1 node with postgres tag..."
  INFRA1=$(docker node ls --format '{{.Hostname}}' | grep "infra-1")
  docker node update --label-add postgres=true $INFRA1
}

main() {
  local hostname
  hostname=$(hostname)
  echo "[*] Configuring Swarn Node: $hostname..."

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

  echo "[*] Copying environment variables..."
  cp -f /tmp/variables.env "$APP_PATH_CONF/.env"
  shopt -s nullglob
  cp -f /tmp/vars-*.env "$APP_PATH_CONF/"
  shopt -u nullglob
  
  echo "[*] Configuring Swarn Node: $hostname...DONE"
}

main
