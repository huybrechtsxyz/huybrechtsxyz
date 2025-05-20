#!/bin/bash

# STOP DEVELOPMENT ENVIRONMENT
cd "/srv/app"
echo "Stopping development environment..."
docker stack rm app
