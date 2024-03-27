# Environments, Secrets and Variables
About environments and their required secrets and environment variables. \
Variables and secrets are set in the github settings per environment. \
For each environment set the required values.

## Environments
An overview of the environments

INFO | DO NOT FORGET TO ASSIGN THE DNS REDIRECTION!

### Development
The development environment on the local machine.
All data is in the appsettings or secrets.json file.

### Test
The test environment. Runs by CI-CD. Unstable environment that gets updated after the CI-CD workflow ran.
> test.huybrechts.xyz

### Staging
The staging environment. Once test is approved, manually launch the workflow action to update the environment.
> staging.huybrechts.xyz

### Production
The production environment. Once staging is approved, manually launch the workflow action to update the environment.
> huybrechts.xyz

## Variables and Secrets
The secrets and variables that are set for each environment.

### Dotnet environment variables
For the .Net runtime environment.

> ASPNETCORE_ENVIRONMENT=[development,test,staging,production]
> DOTNET_RUNNING_IN_CONTAINER=true
> DOTNET_CLI_TELEMETRY_OPTOUT '1'
> DOTNET_EnableDiagnostics=0

### Application environment variables

**Used by Let's Encrypt for refreshing the https certificate**
> APP_CERT_SERVER=https://acme-staging-v02.api.letsencrypt.org/directory
> APP_CERT_SERVER=https://acme-v02.api.letsencrypt.org/directory
> APP_CERT_MAIL=webmaster@huybrechts.xyz

**Used by the server and application for service configuration**
> APP_HOST_DOMAIN=[,test,staging].huybrechts.xyz
> APP_HOST_SERVER=$SERVERIP
> APP_HOST_PORT=22
> APP_HOST_USERNAME=[the-chosen-username-secret]
> APP_HOST_PASSWORD=[the-chosen-password-secret]
> APP_HOST_EMAIL=[the-chosen-email-secret]
> APP_DATA_PROVIDER=[None, SqlLite, SqlServer, PostgreSQL]
> APP_DATA_URL={connectionstring}
> APP_DATA_NAME=applicationdb
> APP_DATA_USERNAME=[the-chosen-username-secret]
> APP_DATA_PASSWORD=[the-chosen-password-secret]

**Used for the docker container repository**
> REGISTRY_USERNAME=[the-chosen-docker-username]
> REGISTRY_PASSWORD=[the-chosen-docker-password]

### Postgres variables
Postgres database environment variables, set with secrets.

**Used by postgres**
> PGDATA=/var/lib/postgresql/data/pgdata // Refers to app/data/pgdata
> POSTGRES_DB_FILE=/run/secrets/app_data_name
> POSTGRES_USER
> POSTGRES_USER_FILE=/run/secrets/app_host_username
> POSTGRES_PASSWORD
> POSTGRES_PASSWORD_FILE=/run/secrets/app_host_password

**Used by PGAdmin**
> POSTGRES_PASSWORD=$APP_HOST_PASSWORD
> PGADMIN_DEFAULT_EMAIL=$APP_HOST_USERNAME
> PGADMIN_DEFAULT_PASSWORD=$APP_HOST_PASSWORD
