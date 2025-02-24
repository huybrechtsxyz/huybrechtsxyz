#!/bin/bash

# STOP DEVELOPMENT ENVIRONMENT
cd "/app"
echo "Stopping development environment..."
docker stack rm app
