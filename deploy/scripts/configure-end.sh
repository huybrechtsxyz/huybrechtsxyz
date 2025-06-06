#!/bin/bash

set -e
source /tmp/variables.env
source /tmp/secrets.env

# Set environment file
cp -f /tmp/variables.env /opt/app/.env

# Set permissions for the app directory does not have to be done here
# Correct permissions for the app directory are set in the startenv.sh script
# This would break the permissions for the app directory when its running
# chown -R 755 /opt/app

# Remove temporary files
#rm -f /tmp/variables.env
#rm -f /tmp/secrets.env
