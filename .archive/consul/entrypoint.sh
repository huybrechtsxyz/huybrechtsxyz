#!/bin/bash

# Get the Swarm Cluster ID
DOCKER_SWARM_IP=$(docker info --format '{{.Swarm.Cluster.ID}}')

# Get the Swarm Cluser Manager Count
DOCKER_SWARM_MANAGERS=$(docker node ls --filter "role=manager" -q | wc -l)

# Check if Swarm is active and the Cluster ID is available
if [ -z "$DOCKER_SWARM_IP" ]; then
  echo "Error: Unable to retrieve Swarm Cluster ID. Is Swarm active on this node?"
  exit 1
fi

echo "Swarm Cluster ID: $DOCKER_SWARM_IP"

# Export the variable to be available for Docker Compose
export DOCKER_SWARM_IP
export DOCKER_SWARM_MANAGERS