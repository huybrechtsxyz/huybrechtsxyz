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

# Example usage of the function
process_file_env_vars 'KC_.*_FILE'

# Import the preprocessed realm JSON
# /opt/keycloak/bin/kc.sh import --file /tmp/custom-realm.json --override false

# Pass all command parameters
exec /opt/keycloak/bin/kc.sh start "$@"
