#!/bin/bash
set -euo pipefail

# Default stack name
parse_options "$@"

# Default stack name if not set
STACK="${STACK:-app}"

# Define the root path
APP_PATH="/opt/app"
source "$APP_PATH/scripts/functions.sh"

log INFO "[*] Stopping DEVELOPMENT environment..."

# Remove stack(s)
if [[ -z "$STACK" ]]; then
    for dir in "$APP_PATH"/*/; do
        service_name=$(basename "$dir")
        log INFO "[*] Removing $service_name stack..."
        docker stack rm "$service_name"
    done
else
    log INFO "[*] Removing $STACK stack..."
    docker stack rm "$STACK"
fi

log INFO "[*] Stopping DEVELOPMENT environment...DONE"