#!/bin/bash

source /opt/app/.env

# Parse options
ROLE=""
SERVICES=()
while [[ $# -gt 0 ]]; do
  case "$1" in
    -r)
      ROLE="$2"
      shift 2
      ;;
    -s)
      shift
      while [[ $# -gt 0 && "$1" != -* ]]; do
        SERVICES+=("$1")
        shift
      done
      ;;
    *)
      echo "Usage: $0 [-r role] [-s service1 service2 ...]"
      exit 1
      ;;
  esac
done

echo "[*] Stopping services..."

cd "$APP_PATH" || exit 1

for dir in "$APP_PATH"/*/; do
  METADATA_FILE="${dir}metadata.json"

  if [ ! -f "$METADATA_FILE" ]; then
    continue
  fi

  META_ROLE=$(jq -r .role "$METADATA_FILE")
  META_SERVICE=$(jq -r .service "$METADATA_FILE")

  if [[ -n "$ROLE" && "$ROLE" != "$META_ROLE" ]]; then
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
      continue
    fi
  fi

  echo "Stopping service stack: $META_SERVICE"
  docker stack rm "$META_SERVICE"
done
