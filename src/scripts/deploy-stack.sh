#!/bin/bash

# Ensure the script exits on errors
set -e

# Validate that the ENVIRONMENT variable is set
: "${ENVIRONMENT:?Environment variable ENVIRONMENT is not set. Please set it before running the script.}"
echo "Deploying to environment: $ENVIRONMENT"

# The minio path
echo "S3 Storage path: $MINIO_DATA" 
export MINIO_DATA

# Function to retrieve overlay network IP
get_overlay_network_ip() {
  local network_name="$1"
  docker network inspect "$network_name" --format '{{range .IPAM.Config}}{{.Gateway}}{{end}}'
}

# Retrieve the overlay network IP
DOCKER_PUBLIC_NAME="public"
DOCKER_PUBLIC_IP=$(get_overlay_network_ip "$DOCKER_PUBLIC_NAME")
DOCKER_PRIVATE_NAME="private"
DOCKER_PRIVATE_IP=$(get_overlay_network_ip "$DOCKER_PRIVATE_NAME")

# Check if IP retrieval was successful
if [[ -z "$DOCKER_PUBLIC_IP" ]]; then
  echo "Failed to retrieve the IP for the overlay network: $DOCKER_PUBLIC_NAME"
  exit 1
fi
if [[ -z "$DOCKER_PRIVATE_IP" ]]; then
  echo "Failed to retrieve the IP for the overlay network: $DOCKER_PRIVATE_NAME"
  exit 1
fi

# Export the overlay network IP as an environment variable
export DOCKER_PUBLIC_IP
echo "Overlay $DOCKER_PUBLIC_NAME network IP: $DOCKER_PUBLIC_IP"
export DOCKER_PRIVATE_IP
echo "Overlay $DOCKER_PRIVATE_NAME network IP: $DOCKER_PRIVATE_IP"

# Set the application path
cd /app

# Set the appropriate security on the application subdirectories
echo "Setting permissions..."
sudo chmod -R 600 /app/traefik/data
sudo chmod -R 777 /app/traefik/logs
sudo chmod -R +x /app/scripts

# Redeploy the new Docker Compose stack
echo "Deploying Docker stack..."
docker stack deploy -c compose.${ENVIRONMENT}.yml --env-file ${ENVIRONMENT}.env app &> /app/deploy-stack.log

echo "Docker stack deployed successfully."
