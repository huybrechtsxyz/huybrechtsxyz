#!/bin/sh

# Loading environment variables from *_FILE references (POSIX compliant)
load_secret_files() {
  echo "[INFO] Loading environment variables from *_FILE references..."

  # Loop over environment variables ending in _FILE
  env | grep -E '^[A-Za-z_][A-Za-z0-9_]*_FILE=' | cut -d= -f1 | while read -r var; do
    base_var=$(printf '%s' "$var" | sed 's/_FILE$//')

    # Get the file path
    # Use parameter expansion instead of eval
    file_path=$(env | grep "^${var}=" | cut -d= -f2-)
    echo "[INFO] Processing variable: $var (file path: $file_path)"

    if [ -f "$file_path" ]; then
      value=$(cat "$file_path")
      echo "[INFO] Setting variable: $base_var (from file content)"
      # POSIX compliant way to export variable dynamically
      # Use 'export' with 'set' to assign variable by name
      # Here we use 'set' and 'eval' carefully:
      eval "$base_var=\$(printf '%s' \"$value\")"
      export "$base_var"
    else
      echo "[WARNING] File '$file_path' specified in $var does not exist or is not a regular file."
    fi
  done

  echo "[INFO] Done loading environment variables."
}

# Acts as envsubst when not available in image (POSIX compliant)
substitute_env_vars() {
  input_file=$1
  output_file=$2

  if [ -z "$input_file" ] || [ -z "$output_file" ]; then
    echo "Usage: substitute_env_vars input-file output-file"
    return 1
  fi

  # Read input file line by line
  # Use 'sed' to escape backslashes, double quotes and backticks for safe eval
  while IFS= read -r line || [ -n "$line" ]; do
    # Escape backslash, double quote and backtick for safe eval echo
    escaped_line=$(printf '%s\n' "$line" | sed 's/\\/\\\\/g; s/"/\\"/g; s/`/\\`/g')
    eval "echo \"$escaped_line\""
  done < "$input_file" > "$output_file"
}
