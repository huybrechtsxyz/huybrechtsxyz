#!/bin/sh

process_file_env_vars() {
    local pattern=$1

    # Ensure the pattern is provided
    if [[ -z $pattern ]]; then
        echo "ERR: No pattern provided to process_file_env_vars"
        return 1
    fi

    # Find suitable variables matching the provided pattern
    local lines=$(printenv | grep -o "$pattern")
    echo "$lines"

    # Split into array
    local vars=($lines)

    # Enumerate variable names
    for var in "${vars[@]}"; do
        # Output variable, trim the _FILE suffix
        # e.g. KC_DB_PASSWORD_FILE -> KC_DB_PASSWORD
        local outvar="${var%_FILE}"

        # Variable content = file path
        local file="${!var}"

        # Empty value -> warn but don't fail
        if [[ -z $file ]]; then
            echo "WARN: $var specified but empty"
            continue
        fi

        # File exists
        if [[ -e $file ]]; then
            # Read contents
            local content=$(cat "$file")
            # Export contents if non-empty
            if [[ -n $content ]]; then
                export "$outvar"="$content"
                echo "INFO: exported $outvar from $var"
            # Empty contents, warn but don't fail
            else
                echo "WARN: $var -> $file is empty"
            fi
        # File is expected but not found. Very likely a misconfiguration, fail early
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
process_file_env_vars '*_FILE'

# Substitute envvars for in the following files
substitute_env_vars "/tmp/realm.template.json" "/tmp/realm.json"

# Import the preprocessed realm JSON
/opt/keycloak/bin/kc.sh import --file /tmp/realm.json --override false

# Pass all command parameters
exec /opt/keycloak/bin/kc.sh start "$@"
