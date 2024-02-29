# Environments, Secrets and Variables
About environments and the required secrets and environment variables. \
Variables are set in the github settings per environment. \
For each environment set the required secrets and variables.

## Environments
An overview of the environments

INFO | DO NOT FORGET TO ASSIGN THE DNS REDIRECTION!

### Development
The local development environment.

### Test
The test environment. Runs by CI-CD. Unstable.
> test.huybrechts.xyz

### Staging
The staging environment. Once development is approved, manually launch the action.
> staging.huybrechts.xyz

### Production
The production environment. Once staging is approved, manually launch the action.
> huybrechts.xyz

## Variables
The secrets and variables that are set for each environment.

### Dotnet environment variables
For the .Net runtime environment.

> ASPNETCORE_ENVIRONMENT=[development,staging,production]
> DOTNET_RUNNING_IN_CONTAINER=true
> DOTNET_CLI_TELEMETRY_OPTOUT '1'
> DOTNET_EnableDiagnostics=0

### Let's Encrypt environment variables

> APP_CERT_SERVER=https://acme-staging-v02.api.letsencrypt.org/directory
> APP_CERT_SERVER=https://acme-v02.api.letsencrypt.org/directory
> APP_CERT_MAIL=webmaster@huybrechts.xyz

### Postgress variables
Postgress database environment variables, set with crets

> POSTGRES_PASSWORD=$APP_HOST_PASSWORD
> PGADMIN_DEFAULT_EMAIL=$APP_HOST_USERNAME
> PGADMIN_DEFAULT_PASSWORD=$APP_HOST_PASSWORD

## Secret management
The host name is set by environment

> APP_HOST_DOMAIN=develop.huybrechts.xyz
> APP_HOST_DOMAIN=staging.huybrechts.xyz
> APP_HOST_DOMAIN=huybrechts.xyz

The host details for user, server, and port

> APP_HOST_SERVER=$IP
> APP_HOST_PORT=22
> APP_HOST_USERNAME=username  // Copied to docker
> APP_HOST_PASSWORD=password  // Copied to docker
> APP_HOST_EMAIL=a@b.com      // Copied to docker

Container registry secrets

> REGISTRY_USERNAME=dockerusername
> REGISTRY_PASSWORD=dockerpassword

ProgreSql server secrets

> APP_DBASE_NAME=applicationdb  // Copied to docker
