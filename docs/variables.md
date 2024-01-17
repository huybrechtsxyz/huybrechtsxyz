# Environments, Secrets and Variables

## Environments

### Development

### Staging

### Production

## Variables

Dotnet environment variables

> ASPNETCORE_ENVIRONMENT=development,staging,production
> DOTNET_RUNNING_IN_CONTAINER=true
> DOTNET_CLI_TELEMETRY_OPTOUT '1'
> DOTNET_EnableDiagnostics=0

Lets Encrypt environment variables

> APP_CERT_SERVER=https://acme-staging-v02.api.letsencrypt.org/directory
> APP_CERT_SERVER=https://acme-v02.api.letsencrypt.org/directory
> APP_CERT_MAIL=webmaster@huybrechts.xyz

Application Variables

> APP_ENVIRONMENT=development

## Build Secrets

> APP_HOST_DOMAIN=develop.huybrechts.xyz

> APP_HOST_SERVER
> APP_HOST_PORT
> APP_HOST_USERNAME
> APP_HOST_PASSWORD

Container registry secrets

> REGISTRY_USERNAME
> REGISTRY_PASSWORD
