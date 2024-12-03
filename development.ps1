<#
Secrets examples
    {
        "KEYCLOAK_USER": "admin",
        "KEYCLOAK_PASSWORD": "password",
        "MINIO_ROOT_USER": "admin",
        "MINIO_ROOT_PASSWORD": "password",
        "POSTGRES_USER": "admin",
        "POSTGRES_PASSWORD": "password",
        "PGADMIN_USER": "admin",
        "PGADMIN_PASSWORD": "password",
        "VERSIO_USERNAME": "",
        "VERSIO_PASSWORD": "",
    }
#>

# FUNCTION: Extract zipfile for local development
function Expand-ZipFile {
    param (
        [string]$zipFile,
        [string]$extractPath
    )

    # Check if the zip file exists
    if (Test-Path -Path $zipFile) {
        # Load the compression assembly and extract the zip file
        Add-Type -AssemblyName System.IO.Compression.FileSystem
        [System.IO.Compression.ZipFile]::ExtractToDirectory($zipFile, $extractPath)
        Write-Host "Zip file extracted successfully to $extractPath"
    } else {
        Write-Error "Zip file not found at $zipFile"
    }
}

# FUNCTION: Check if Docker is available by running the `docker` command
function Get-Docker {
    try {
        # Try running 'docker --version' to check if Docker is installed and available
        $dockerVersion = & docker --version 2>&1
        # If Docker is available, $dockerVersion should contain version information, otherwise it throws an error
        if ($dockerVersion -match "Docker version") {
            Write-Host " -- Docker is available"
            # Check if Docker is running
            try {
                $output = docker run --rm hello-world 2>&1
                if ($output -like "*docker: error during connect*") {
                    Write-Host " -- Docker is not running"
                    return $false
                } else {
                    Write-Host " -- Docker is running"
                    return $true
                }
            } catch {
                Write-Host " -- Docker is not running."
                return $false
            }
        } else {
            Write-Host " -- Docker not found"
            return $false
        }
    } catch {
        # Handle the case where Docker isn't available
        Write-Host " -- Docker is not installed."
        return $false
    }
}

# FUNCTION: Configure required docker networks
function Set-Docker {
    # Check if the current node is already part of a swarm
    $swarmState = docker info --format '{{.Swarm.LocalNodeState}}'

    if ($swarmState -eq "active") {
        Write-Host "Swarm is already initialized." -ForegroundColor Green
    } else {
        # Initialize a new swarm
        Write-Host "Initializing a new Docker Swarm..." -ForegroundColor Yellow
        $initResult = docker swarm init

        if ($initResult -like "*is now a manager*") {
            Write-Host "Swarm initialized successfully!" -ForegroundColor Green
        } else {
            Write-Host "Swarm initialization failed. Check Docker logs for more information." -ForegroundColor Red
            Write-Host $initResult
            exit 1
        }
    }

    # Check and create the "traefik" network if it doesn't exist
    $traefikNetwork = "public"
    if (!(docker network ls --format '{{.Name}}' | Select-String -Pattern "^$traefikNetwork$")) {
        Write-Host "Creating external network '$traefikNetwork'..."
        docker network create --driver overlay --attachable $traefikNetwork
    } else {
        Write-Host "Network '$traefikNetwork' already exists."
    }

    # Check and create the "intranet" network if it doesn't exist
    $intranetNetwork = "private"
    if (!(docker network ls --format '{{.Name}}' | Select-String -Pattern "^$intranetNetwork$")) {
        Write-Host "Creating bridge network '$intranetNetwork'..."
        docker network create --driver overlay --attachable $intranetNetwork
    } else {
        Write-Host "Network '$intranetNetwork' already exists."
    }
}

# FUNCTION: Read jsons and create docker secrets

function Update-Secrets {
    $SecretFilePath = "C:\Users\vhuybrec\AppData\Roaming\Microsoft\UserSecrets\acbede3b-f8f8-41b2-ad78-d1f3e176949f\secrets.json"
    if (-not (Test-Path -Path $SecretFilePath)){
        $SecretFilePath = "C:\Users\vince\AppData\Roaming\Microsoft\UserSecrets\acbede3b-f8f8-41b2-ad78-d1f3e176949f\secrets.json"
    }
    if (-not (Test-Path -Path $SecretFilePath)){
        throw 'User secrets not found'
    }

    $Secrets = Get-Content -Path $SecretFilePath | ConvertFrom-Json
    foreach ($Key in $Secrets.PSObject.Properties.Name) {
        $Value = $Secrets.$Key

        # Remove the secret if it already exists
        if (docker secret inspect $Key > $null 2>&1) {
            Write-Host "Removing existing secret: $Key"
            docker secret rm $Key
        }

        # Create the Docker secret
        Write-Host "Creating secret: $Key"
        $Value | docker secret create $Key - > $null 2>&1
    }
}

# FUNCTION: Configure and run Consul
function Invoke-Consul {
    Write-Host 'Configuring CONSUL ... for DOCKER'
    $consulDir = "$baseDir/consul"
    $consulConf = "$consulDir/conf"
    $consulData = "$consulDir/data"
    New-Item -ItemType Directory -Path $consulDir, $consulConf, $consulData -Force
    $consulDir = Resolve-Path -Path $consulDir
    Copy-Item -Path "./src/consul/*" -Destination $consulConf -Recurse
    Write-Host 'Configuring CONSUL ... Done'
}

# FUNCTION: Configure and run Keycloak
function Invoke-Keycloak {
    Write-Host 'Configuring KEYCLOAK ... for DOCKER'
    
    Write-Host 'Configuring KEYCLOAK ... Done'
}

# FUNCTION: Configure and run MinIO
function Invoke-Minio {
    Write-Host 'Configuring MINIO ... for DOCKER'
    $minioDir = "$baseDir/minio"
    $minioData = "$minioDir/data"
    New-Item -ItemType Directory -Path $minioDir, $minioData -Force
    $minioDir = Resolve-Path -Path $minioDir
    Write-Host 'Configuring MINIO ... Done'    
}

# FUNCTION: Configure and run Postgres
function Invoke-Postgres {
    Write-Host 'Configuring POSTGRESQL ... for DOCKER'
    $postgresDir = "$baseDir/postgres"
    $postgresData = "$postgresDir/data"
    $postgresAdmin = "$postgresDir/admin"
    $postgresBackup = "$postgresDir/backups"
    New-Item -ItemType Directory -Path $postgresDir, $postgresData, $postgresAdmin, $postgresBackup -Force
    Write-Host 'Configuring POSTGRESQL ... Done'
}

# FUNCTION: Configure and run Traefik
function Invoke-Traefik {
    Write-Host 'Configuring TRAEFIK ... for DOCKER'
    $traefikDir = "$baseDir/traefik"
    $traefikConf = "$traefikDir/conf"
    $traefikData = "$traefikDir/data"
    $traefikLogs = "$traefikDir/logs"
    New-Item -ItemType Directory -Path $traefikDir, $traefikData, $traefikConf, $traefikLogs -Force
    $traefikConf = Resolve-Path -Path $traefikConf
    Copy-Item -Path "./src/traefik/*" -Destination $traefikConf -Recurse
    Write-Host 'Configuring TRAEFIK ... Done'
}

#
# START DEVELOPMENT SERVICES
#

# Start development environment
Write-Output 'Starting DEVELOPMENT environment...'

# Check if Docker is available by running the `docker` command
Write-Output 'Validating docker...'
$docker = Get-Docker
if ($docker -ne 'true') {
    throw 'Docker is not installed or active'
}
Set-Docker
Update-Secrets

# Basic paths
Write-Output 'Creating APP directories...'
$baseDir = './.app'
$scriptDir = "$baseDir/scripts"
New-Item -ItemType Directory -Path $baseDir, $scriptDir -Force
Copy-Item -Path "./src/scripts/*" -Destination $scriptDir -Recurse

# Configure and run services
Invoke-Consul
Invoke-Traefik
Invoke-Minio
Invoke-Postgres
Invoke-Keycloak

# Debug and test
$composeFile = "./src/compose.yml"
$environmentFile = "./src/develop.env"

# Importing the .env file and exporting the variables
$env:HOSTNAME=$env:COMPUTERNAME
if (Test-Path $environmentFile) {
    Get-Content $environmentFile | ForEach-Object {
        $key, $value = $_ -split '='
        [System.Environment]::SetEnvironmentVariable($key.Trim(), $value.Trim(), [System.EnvironmentVariableTarget]::Process)
    }
}

Write-Host "Validating Docker Compose..."
docker compose -f $composeFile --env-file $environmentFile config
if ($LASTEXITCODE -ne 0) {
    Write-Host "Error: Invalid Docker Compose configuration or environment file." -ForegroundColor Red
    exit 1
}

Write-Host "Starting Docker Swarm..."
Write-Error "Multiple private IPv4 addresses found. Please configure one with 'bind' and/or 'advertise'."
#docker compose -f $composeFile up -d
docker stack deploy -c $composeFile app

# DEBUG AND TEST
Start-Process -FilePath "msedge.exe" `
    "--inprivate",                          # Open in InPrivate mode
    "--start-maximized",                    # Start maximized
    "--new-window"                          # Open in a new window

# Stopping the development environment
Pause 'Press any key to stop debugging'
Write-Output 'Stopping DEVELOPMENT environment...'
Stop-Process -Name 'msedge' -ErrorAction Ignore

# Stop Docker Compose
Write-Host "Stopping Docker Swarm..."
#docker compose -f $composeFile down
docker stack rm app

# Stop the development environment
Write-Output 'DEVELOPMENT environment stopped.'