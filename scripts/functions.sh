#!/bin/bash

createpaths_from_metadata() {
  local service="$1"
  local metadata_file="$APP_PATH/$service/conf/metadata.json"

  if [[ ! -f "$metadata_file" ]]; then
    echo "[x] Error: Metadata file '$metadata_file' not found."
    return 1
  fi

  local paths
  paths=$(jq -c '.servicepaths[]?' "$metadata_file")

  if [[ -z "$paths" ]]; then
    echo "[*] No servicepaths defined for service '$service'"
    return 0
  fi

  while IFS= read -r path_entry; do
    local subpath chmod dirpath
    subpath=$(echo "$path_entry" | jq -r '.path')
    chmod=$(echo "$path_entry" | jq -r '.chmod')
    dirpath="$APP_PATH/$service/$subpath"

    if [[ ! -d "$dirpath" ]]; then
      echo "[*] Creating directory: $dirpath"
      if ! mkdir -p "$dirpath"; then
        echo "[x] Error: Failed to create directory '$dirpath'"
        continue
      fi
    fi

    echo "[*] Setting permissions on $dirpath to $chmod"
    sudo chmod -R "$chmod" "$dirpath"
  done <<< "$paths"

  return 0
}
