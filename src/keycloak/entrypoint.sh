#!/bin/sh
set -e

load_secret_files() {
  echo "[INFO] Loading environment variables from *_FILE references..."

  # Loop over environment variables ending in _FILE
  for var in $(env | grep -E '^[A-Za-z_][A-Za-z0-9_]*_FILE=' | cut -d= -f1); do
    base_var="${var%_FILE}"

    # Get the file path
    file_path=$(eval echo "\$$var")
    echo "[INFO] Processing variable: $var (file path: $file_path)"

    if [ -f "$file_path" ]; then
      value=$(cat "$file_path")
      echo "[INFO] Setting variable: $base_var (from file content)"
      # Assign the variable safely
      eval "$base_var=\"\$value\""
      export "$base_var"
    else
      echo "[WARNING] File '$file_path' specified in $var does not exist or is not a regular file."
    fi
  done

  echo "[INFO] Done loading environment variables."
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
load_secret_files

# Substitute envvars for in the following files
substitute_env_vars "/tmp/realm.template.json" "/tmp/realm.json"

# Wait for Postgres readiness
echo "[INFO] Waiting for Postgres to be ready..."
until nc -z -v -w5 "${KC_DB_URL_HOST:-postgres}" 5432; do
  echo "[INFO] Postgres is unavailable - sleeping"
  sleep 2
done
echo "[INFO] Postgres is ready!"

# Import the preprocessed realm JSON
#/opt/keycloak/bin/kc.sh import --file /tmp/realm.json --override false
if [ ! -f "/opt/keycloak/data/imported.flag" ]; then
  echo "[INFO] Importing realm..."
  /opt/keycloak/bin/kc.sh import --file /tmp/realm.json --override false
  touch /opt/keycloak/data/imported.flag
else
  echo "[INFO] Realm already imported. Skipping import."
fi

# Pass all command parameters
exec /opt/keycloak/bin/kc.sh start "$@"
