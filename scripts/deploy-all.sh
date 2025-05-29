#!/bin/bash
set -e

source /opt/app/functions.sh

log INFO "[*] Starting ALL services..."

deploy-stack.sh -e shared -s traefik
deploy-stack.sh -e shared -s consul
deploy-stack.sh -e shared -s minio

log INFO "[+] Starting ALL services...DONE"
log INFO "[+] Listing ALL services..."
docker service ls
