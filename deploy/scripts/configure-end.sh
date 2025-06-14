#!/bin/bash

set -e
source /tmp/app/variables.env
source /tmp/app/secrets.env

# Set environment file is done in the deploy stack script
# cp -f /tmp/app/variables.env /opt/app/.env
# Set permissions for the app directory does not have to be done here
# Correct permissions for the app directory are set in the start script
# This would break the permissions for the app directory when its running
# chown -R 755 /opt/app

# Remove temporary files
rm -f /tmp/app
