#!/bin/bash
set -euo pipefail

# Initialize script
SCRIPT_PATH="$(cd "$(dirname "$(readlink -f "${BASH_SOURCE[0]}")")" && pwd)"
source "$SCRIPT_PATH/utilities.sh"
parse_options "$@"
load_envfile "$ENV_FILE.env"

# Default stack name if not set
STACK="${STACK:-app}"

log INFO "[*] Stopping $STACK environment..."

# Remove stack(s)
if [[ -z "$STACK" ]]; then
    for dir in "$APP_PATH_CONF"/*/; do
        service_name=$(basename "$dir")
        log INFO "[*] Removing $service_name stack..."
        docker stack rm "$service_name"
    done
else
    log INFO "[*] Removing $STACK stack..."
    docker stack rm "$STACK"
fi

log INFO "[*] Stopping $STACK environment...DONE"