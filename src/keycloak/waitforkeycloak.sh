#!/bin/sh
set -e

# Wait for Keycloak to be up
TIMEOUT=120

echo "[INFO] Waiting for Keycloak at $KEYCLOAK_HOST:$KEYCLOAK_PORT..."
while ! nc -z "$KEYCLOAK_HOST" "$KEYCLOAK_PORT"; do
  TIMEOUT=$((TIMEOUT-1))
  if [ "$TIMEOUT" -le 0 ]; then
    echo "[ERROR] Keycloak did not become ready in time!"
    exit 1
  fi
  echo "[INFO] Keycloak unavailable - sleeping 2s..."
  sleep 2
done

echo "[INFO] Keycloak is accepting TCP connections."

# Wait for the admin API to respond with HTTP 200
TIMEOUT=60
while ! curl -s -o /dev/null -w "%{http_code}" "http://$KEYCLOAK_HOST:$KEYCLOAK_PORT/auth/" | grep -q "200"; do
  TIMEOUT=$((TIMEOUT-1))
  if [ "$TIMEOUT" -le 0 ]; then
    echo "[ERROR] Keycloak HTTP API did not become ready in time!"
    exit 1
  fi
  echo "[INFO] Keycloak HTTP API unavailable - sleeping 2s..."
  sleep 2
done

echo "[INFO] Keycloak HTTP API is ready."

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

load_secret_files

# Example: Use kcadm.sh to do something
/opt/keycloak/bin/kcadm.sh config credentials \
  --server "http://$KEYCLOAK_HOST:$KEYCLOAK_PORT" \
  --realm master \
  --user "$KC_BOOTSTRAP_ADMIN_USERNAME" \
  --password "$KC_BOOTSTRAP_ADMIN_PASSWORD"

# Import the preprocessed realm JSON
if [ ! -f "/opt/keycloak/data/imported.flag" ]; then
  echo "[INFO] Importing realm..."
  /opt/keycloak/bin/kc.sh import --file /tmp/realm.json --override false
  touch /opt/keycloak/data/imported.flag
else
  echo "[INFO] Realm already imported. Skipping import."
fi

echo "[INFO] Keycloak initialization completed."
