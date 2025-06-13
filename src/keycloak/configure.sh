#!/bin/bash
set -e
echo "[*] Deploying KEYCLOAK to remote server $(hostname)..."
create_service_paths "keycloak"
echo "[*] Deploying KEYCLOAK to remote server $(hostname)...DONE"