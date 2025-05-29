#!/bin/bash
set -e

source /opt/app/functions.sh

log INFO "[*] Removing ALL services..."

# Remove in reverse order of dependencies
docker stack rm minio
docker stack rm consul
docker stack rm traefik

log INFO "[+] Remove request sent for all services."
log INFO "[+] Waiting for cleanup to complete..."

# Optionally wait for services to fully shut down
sleep 10
docker service ls
