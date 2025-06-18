#!/bin/bash
set -euo pipefail

log INFO "Starting application uninstall..."

return 1;

# # Initialize script
# SCRIPT_PATH="$(cd "$(dirname "$(readlink -f "${BASH_SOURCE[0]}")")" && pwd)"
# source "$SCRIPT_PATH/functions.sh"

# ENV_FILE="/etc/app/shared.env"
# load_envfile "$ENV_FILE"

# MOUNT_BASES=("/var/lib" "/srv" "/mnt")
# APP_PREFIXES=("data" "logs" "serv")

# log INFO "Starting application uninstall...COMPLETE"


# # Default stack name if not set
# STACK="${STACK:-app}"

# log INFO "[*] Stopping $STACK environment..."

# # Remove stack(s)
# if [[ -z "$STACK" ]]; then
#     for dir in "$APP_PATH_CONF"/*/; do
#         service_name=$(basename "$dir")
#         log INFO "[*] Removing $service_name stack..."
#         docker stack rm "$service_name"
#     done
# else
#     log INFO "[*] Removing $STACK stack..."
#     docker stack rm "$STACK"
# fi

#!/bin/bash
# set -euo pipefail

# log() {
#   local level="$1"; shift
#   local msg="$*"
#   local timestamp
#   timestamp=$(date +"%Y-%m-%d %H:%M:%S")
#   case "$level" in
#     INFO)  echo -e "[$timestamp] [\033[1;34mINFO\033[0m]  - $msg" ;;
#     WARN)  echo -e "[$timestamp] [\033[1;33mWARN\033[0m]  - $msg" ;;
#     ERROR) echo -e "[$timestamp] [\033[1;31mERROR\033[0m] - $msg" >&2 ;;
#     *)     echo -e "[$timestamp] [UNKNOWN] - $msg" ;;
#   esac
# }

# log INFO "Starting application uninstall..."

# # Constants
# ENV_FILE="/etc/app/.env"
# PROFILE_FILE="/etc/profile.d/app_paths.sh"
# MOUNT_BASES=("/var/lib" "/srv" "/mnt")
# APP_PREFIXES=("data" "logs" "serv")

# # Step 1: Unmount mount points (reverse order to handle bind mounts)
# log INFO "Unmounting bind and data mounts..."
# grep -E '(/var/lib|/srv|/mnt)/' /etc/fstab | awk '{print $2}' | sort -r | while read -r mountpoint; do
#   if mountpoint -q "$mountpoint"; then
#     log INFO "Unmounting $mountpoint"
#     if umount -f "$mountpoint"; then
#       log INFO "Unmounted $mountpoint"
#     else
#       log WARN "Failed to unmount $mountpoint"
#     fi
#   fi
# done

# # Step 2: Clean fstab entries
# log INFO "Backing up and cleaning /etc/fstab..."
# cp /etc/fstab /etc/fstab.bak
# grep -vE '(/var/lib|/srv|/mnt)/' /etc/fstab.bak > /etc/fstab
# log INFO "Cleaned /etc/fstab (backup at /etc/fstab.bak)"

# # Step 3: Remove directories
# log INFO "Removing application directories..."
# for base in "${MOUNT_BASES[@]}"; do
#   for prefix in "${APP_PREFIXES[@]}"; do
#     find "$base" -maxdepth 1 -type d -name "${prefix}*" | while read -r dir; do
#       log INFO "Removing $dir"
#       if rm -rf "$dir"; then
#         log INFO "Deleted $dir"
#       else
#         log WARN "Could not delete $dir"
#       fi
#     done
#   done
# done

# # Step 4: Remove environment and profile files
# if [ -f "$ENV_FILE" ]; then
#   log INFO "Removing env file: $ENV_FILE"
#   rm -f "$ENV_FILE" && log INFO "Removed $ENV_FILE"
# else
#   log WARN "Env file not found: $ENV_FILE"
# fi

# if [ -f "$PROFILE_FILE" ]; then
#   log INFO "Removing profile file: $PROFILE_FILE"
#   rm -f "$PROFILE_FILE" && log INFO "Removed $PROFILE_FILE"
# else
#   log WARN "Profile file not found: $PROFILE_FILE"
# fi

# log INFO "App environment uninstalled successfully."


# log INFO "[*] Stopping $STACK environment...DONE"
