# This script runs the local development environment.
# Run from the root of the repository.
$RootPath = "C:/Users/vince/Sources/huybrechtsxyz"
$SourcePath = "$RootPath/src"
$AppPath = "$RootPath/.app"

# Load the required modules
. "$RootPath/scripts/psfunctions.ps1"

# Start development environment
Write-Host 'Starting DEVELOPMENT environment...'

# Check if Docker is available by running the `docker` command
Write-Host 'Validating docker...'
$docker = Get-Docker
if ($docker -ne 'true') {
    throw 'Docker is not installed or active'
}
Configure-Docker
Update-Secrets

# Basic paths
Write-Host 'Creating APP directories...'
New-Item -ItemType Directory -Path $AppPath -Force
Copy-Item -Path "$SourcePath/compose.yml" -Destination "$AppPath/compose.yml" -Force
Copy-Item -Path "$SourcePath/develop.env" -Destination "$AppPath/.env" -Force

# Importing the .env file and exporting the variables
$composeFile = "$AppPath/compose.yml"
$environmentFile = "$SourcePath/develop.env"
Write-Host "Environment variables are:" 
$env:HOSTNAMEID=$(hostname)
$env:HOSTNAME=$env:COMPUTERNAME
$env:DOCKER_MANAGERS=1
Write-Host " -- HOSTNAME: $env:HOSTNAME" 
Write-Host " -- DOCKER_PUBLIC_IP: $env:DOCKER_PUBLIC_IP" 
Write-Host " -- DOCKER_MANGER_COUNT: $env:DOCKER_MANAGER_COUNT" 
if (Test-Path $environmentFile) {
    Get-Content $environmentFile | ForEach-Object {
        $key, $value = $_ -split '='
        [System.Environment]::SetEnvironmentVariable($key.Trim(), $value.Trim(), [System.EnvironmentVariableTarget]::Process)
        Write-Host " -- $key : $value" 
    }
}

# Configure and run Traefik
function Invoke-Traefik {
    Write-Host 'Configuring TRAEFIK ... for DOCKER'
    $traefikDir = "$AppPath/traefik"
    $traefikConf = "$traefikDir/conf"
    $traefikData = "$traefikDir/data"
    $traefikLogs = "$traefikDir/logs"
    New-Item -ItemType Directory -Path $traefikDir, $traefikConf, $traefikData, $traefikLogs -Force

    $traefikConf = Resolve-Path -Path $traefikConf
    Copy-Item -Path "$SourcePath/traefik/*" -Destination $traefikConf -Recurse
    Remove-Item -Path "$traefikLogs/*" -Recurse -Force

    Apply-Template -templateFile "$APPPATH/traefik/conf/traefik-config.template.yml" -configFile "$APPPATH/traefik/conf/traefik-config.yml"
    Write-Host 'Configuring TRAEFIK ... Done'
}
Invoke-Traefik

# Configure and run CONSUL
function Invoke-Consul {
    Write-Host 'Configuring CONSUL ... for DOCKER'
    $consulDir = "$AppPath/consul"
    $consulConf = "$consulDir/conf"
    $consulData = "$consulDir/data"
    New-Item -ItemType Directory -Path $consulDir, $consulConf, $consulData -Force

    $consulConf = Resolve-Path -Path $consulConf
    Copy-Item -Path "$SourcePath/consul/*" -Destination $consulConf -Recurse
    Write-Host 'Configuring CONSUL ... Done'
}
Invoke-Consul

# Configure and run MINIO
function Invoke-Minio {
    Write-Host 'Configuring MINIO ... for DOCKER'
    $minioDir = "$AppPath/minio"
    $minioConf = "$minioDir/conf"
    $minioData = "$minioDir/data"
    New-Item -ItemType Directory -Path $minioDir, $minioConf, $minioData -Force

    $minioConf = Resolve-Path -Path $minioConf
    Copy-Item -Path "$SourcePath/minio/*" -Destination $minioConf -Recurse
    Write-Host 'Configuring MINIO ... Done'
}
Invoke-Minio

# Configure and run POSTGRES
function Invoke-Postgres {
    Write-Host 'Configuring POSTGRES ... for DOCKER'
    $postgresDir = "$AppPath/postgres"
    $postgresConf = "$postgresDir/conf"
    $postgresData = "$postgresDir/data"
    $postgresAdmin = "$postgresDir/admin"
    $postgresBU = "$postgresDir/backups"
    New-Item -ItemType Directory -Path $postgresDir, $postgresConf, $postgresData, $postgresAdmin, $postgresBU -Force

    $postgresConf = Resolve-Path -Path $postgresConf
    Copy-Item -Path "$SourcePath/postgres/*" -Destination $postgresConf -Recurse
    Write-Host 'Configuring POSTGRES ... Done'
}
Invoke-Postgres

# Configure and run KEYCLOAK
function Invoke-Keycloak {
    Write-Host 'Configuring KEYCLOAK ... for DOCKER'
    $keycloakDir = "$AppPath/keycloak"
    $keycloakConf = "$keycloakDir/conf"
    New-Item -ItemType Directory -Path $keycloakDir, $keycloakConf -Force

    $keycloakConf = Resolve-Path -Path $keycloakConf
    Copy-Item -Path "$SourcePath/keycloak/*" -Destination $keycloakConf -Recurse
    Write-Host 'Configuring KEYCLOAK ... Done'
}
Invoke-Keycloak

Write-Host "Validating Docker Compose..."
docker compose -f $composeFile --env-file $environmentFile config
if ($LASTEXITCODE -ne 0) {
    Write-Host "Error: Invalid Docker Compose configuration or environment file." -ForegroundColor Red
    exit 1
}

Write-Host "Starting Docker Swarm..."
#docker compose -f $composeFile up -d
docker stack deploy -c $composeFile app --detach=true

# DEBUG AND TEST
Start-Process -FilePath "msedge.exe" `
    "http://proxy.$env:DOMAIN_DEV/dashboard/",
    "http://config.$env:DOMAIN_DEV/",
    "http://s3.$env:DOMAIN_DEV",
    "http://db.$env:DOMAIN_DEV/pgamin",
    "--inprivate",
    "--ignore-certificate-errors",
    "--ignore-urlfetcher-cert-requests",
    "--start-maximized",
    #"--start-minimized",
    "--new-window"

# Wait for user input to redeploy or stop the environment
$continue = $true
while ($continue) {
    $key = Read-Host "Press any key to stop debugging or 'r' to redeploy stack"
    
    if ($key -eq 'r' -or $key -eq 'R') {
        Write-Host "Redeploying Docker stack..."
        docker stack deploy -c $composeFile app --detach=true
        Write-Host "Docker stack redeployed with updated compose file."
    }
    elseif ($key -eq '') {
        Write-Host 'Stopping DEVELOPMENT environment...'
        Stop-Process -Name 'msedge' -ErrorAction Ignore
        $continue = $false
    }
}

# Stop Docker Compose
Write-Host "Stopping Docker Swarm..."
#docker compose -f $composeFile down
docker stack rm app

# Stop the development environment
Write-Host 'DEVELOPMENT environment stopped.'
