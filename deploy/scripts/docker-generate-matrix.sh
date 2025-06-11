#!/bin/bash
set -euo pipefail

SERVICES_INPUT="$1"
OUTPUT_PATH="$2"

# Split the input list into an array
IFS=',' read -ra SELECTED_SERVICES <<< "$SERVICES_INPUT"

# Function to check if an item is in the selected list
function is_selected {
  local item="$1"
  for selected in "${SELECTED_SERVICES[@]}"; do
    if [[ "$selected" == "$item" ]]; then
      return 0
    fi
  done
  return 1
}

# Build the matrix
matrix='['
for dir in ./src/*; do
  service=$(basename "$dir")
  json_file="$dir/service.json"
  dockerfile="$(jq -r .dockerfile "$json_file" 2>/dev/null || echo "")"

  if [[ -f "$json_file" && -n "$dockerfile" && -f "$dir/$dockerfile" ]]; then
    if [[ -z "$SERVICES_INPUT" || is_selected "$service" ]]; then
      json=$(cat "$json_file")
      merged=$(echo "$json" | jq --arg service "$service" '. + {service: $service}')
      matrix+="$merged,"
    fi
  fi
done
matrix="${matrix%,}]"

# Write to GitHub output file
echo "matrix=$matrix" >> "$OUTPUT_PATH"
