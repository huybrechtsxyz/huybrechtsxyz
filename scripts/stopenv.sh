#!/bin/bash

# STOP DEVELOPMENT ENVIRONMENT
cd "/opt/app"
echo "Stopping development environment..."
docker stack rm app
