#!/bin/sh
set -e

process_file_env_vars() {
  # Loop over all env vars ending in _FILE
  for var in $(env | awk -F= '/_FILE=/ {print $1}'); do
    # Strip _FILE suffix
    outvar="${var%_FILE}"
    # Get the file path
    file="$(printenv "$var")"
    # Check if empty
    if [ -z "$file" ]; then
      echo "WARN: $var is empty"
      continue
    fi
    # Check if the file exists
    if [ -f "$file" ]; then
      # Read file content
      content="$(cat "$file")"
      # Export variable
      export "$outvar=$content"
      echo "INFO: exported $outvar from $var"
    else
      echo "ERR: $var -> file '$file' not found"
      exit 1
    fi
  done
}

substitute_env_vars() {
  local input_file="$1"
  local output_file="$2"

  if [[ -z "$input_file" || -z "$output_file" ]]; then
    echo "Usage: envsubst_simple input-file output-file"
    return 1
  fi

  while IFS= read -r line; do
    local escaped_line
    escaped_line=$(printf '%s\n' "$line" | sed 's/\\/\\\\/g; s/"/\\"/g; s/`/\\`/g')
    eval "echo \"$escaped_line\""
  done < "$input_file" > "$output_file"
}

# Read all _FILE secret files to environment variables
process_file_env_vars

# Substitute envvars for in the following files
substitute_env_vars "/tmp/realm.template.json" "/tmp/realm.json"

# Import the preprocessed realm JSON
/opt/keycloak/bin/kc.sh import --file /tmp/realm.json --override false

# Pass all command parameters
exec /opt/keycloak/bin/kc.sh start "$@"
