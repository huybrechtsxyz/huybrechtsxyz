 #!/bin/bash

source /opt/app/functions.sh

log INFO "[*] Stopping services..."

APP_PATH="/opt/app"
parse_options "$@"

cd "$APP_PATH" || exit 1

for dir in "$APP_PATH"/*/; do
  METADATA_FILE="${dir}/conf/metadata.json"

  if [ ! -f "$METADATA_FILE" ]; then
    log WARN "[!] Metadata file not found in $dir, skipping..."
    continue
  fi

  META_GROUP=$(jq -r .group "$METADATA_FILE")
  META_SERVICE=$(jq -r .service "$METADATA_FILE")

  if [[ -n "$GROUP" && "$GROUP" != "$META_GROUP" ]]; then
    log INFO "[-] Skipping $META_SERVICE (group mismatch: $META_GROUP)"
    continue
  fi

  if [[ ${#SERVICES[@]} -gt 0 ]]; then
    MATCHED=false
    for svc in "${SERVICES[@]}"; do
      if [[ "$svc" == "$META_SERVICE" ]]; then
        MATCHED=true
        break
      fi
    done
    if [[ "$MATCHED" == false ]]; then
      log INFO "[-] Skipping $META_SERVICE (not in selected service list)"
      continue
    fi
  fi

  log INFO "[+] Stopping service stack: $META_SERVICE"
  docker stack rm "$META_SERVICE"
done
log INFO "[*] Stopping services...DONE"