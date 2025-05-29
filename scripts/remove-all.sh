#!/bin/bash
set -e

source /opt/app/functions.sh

log INFO "[*] Removing ALL services..."

# Remove in reverse order of dependencies
remove-stack.sh -e shared -s postgres
remove-stack.sh -e shared -s minio
remove-stack.sh -e shared -s consul
remove-stack.sh -e shared -s traefik

log INFO "[+] Remove request sent for all services."
log INFO "[+] Waiting for cleanup to complete..."

# Optionally wait for services to fully shut down
sleep 10
docker service ls
