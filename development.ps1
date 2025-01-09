<#
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

    # Check and create the "public" network if it doesn't exist
    $publicNetwork = "public"
    if (!(docker network ls --format '{{.Name}}' | Select-String -Pattern "^$publicNetwork$")) {
        Write-Host "Creating external network '$publicNetwork'..."
        docker network create --subnet=10.0.0.0/24 --driver overlay $publicNetwork
    } else {
        Write-Host "Network '$publicNetwork' already exists."
    }

    $publicDetails = docker network inspect $publicNetwork | ConvertFrom-Json
    #$publicSubnetIP = $publicDetails.IPAM.Config[0].Subnet # For Subnet
    $publicGatewayIP = $publicDetails.IPAM.Config[0].Gateway # For Gateway
    Write-Host "Public Network IP: $publicGatewayIP"
    [System.Environment]::SetEnvironmentVariable("DOCKER_PUBLIC_IP", $publicGatewayIP, [System.EnvironmentVariableTarget]::Process)

    # Check and create the "intranet" network if it doesn't exist
    # $intranetNetwork = "private"
    # if (!(docker network ls --format '{{.Name}}' | Select-String -Pattern "^$intranetNetwork$")) {
    #     Write-Host "Creating bridge network '$intranetNetwork'..."
    #     docker network create --driver overlay $intranetNetwork
    # } else {
    #     Write-Host "Network '$intranetNetwork' already exists."
    # }

    # $privateDetails = docker network inspect $intranetNetwork | ConvertFrom-Json
    # #$privateSubnetIP = $privateDetails.IPAM.Config[0].Subnet # For Subnet
    # $privateGatewayIP = $privateDetails.IPAM.Config[0].Gateway # For Gateway
    # Write-Host "Private Network IP: $privateGatewayIP"
    # [System.Environment]::SetEnvironmentVariable("DOCKER_PRIVATE_IP", $privateGatewayIP, [System.EnvironmentVariableTarget]::Process)
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
        Write-Host "Creating or updating secret: $Key"
        
        # Remove the secret if it already exists
        if (docker secret inspect $Key 2>&1) {
            docker secret rm $Key 2>&1
        }

        # Create the Docker secret
        $TempSecretFile = [System.IO.Path]::GetTempFileName()
        Set-Content -Path $TempSecretFile -Value $Value -NoNewLine
        $WrittenValue = Get-Content -Path $TempSecretFile -Raw
        if ($WrittenValue -ne $Value) {
            throw "The written secret value for '$Key' does not match the original."
        }
        docker secret create $Key $TempSecretFile
        Remove-Item -Path $TempSecretFile
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
    $keycloakDir = "$baseDir/keycloak"
    New-Item -ItemType Directory -Path $keycloakDir -Force
    $keycloakDir = Resolve-Path -Path $keycloakDir
    Copy-Item -Path "./src/keycloak/*" -Destination $keycloakDir -Recurse
    Write-Host 'Configuring KEYCLOAK ... Done'
}

# FUNCTION: Configure and run MinIO
function Invoke-Minio {
    Write-Host 'Configuring MINIO ... for DOCKER'
    $minioDir = "$baseDir/minio"
    $minioConf = "$minioDir/conf"
    $minioData = "$minioDir/data"
    $minioLogs = "$minioDir/logs"
    New-Item -ItemType Directory -Path $minioDir, $minioConf, $minioData, $minioLogs -Force
    $minioConf = Resolve-Path -Path $minioConf
    Copy-Item -Path "./src/minio/*" -Destination $minioConf -Recurse
    Write-Host 'Configuring MINIO ... Done'
}

# FUNCTION: Configure and run Postgres
function Invoke-Postgres {
    Write-Host 'Configuring POSTGRESQL ... for DOCKER'
    $postgresDir = "$baseDir/postgres"
    $postgresConf = "$postgresDir/conf"
    $postgresData = "$postgresDir/data"
    $postgresAdmin = "$postgresDir/admin"
    $postgresBackup = "$postgresDir/backups"
    New-Item -ItemType Directory -Path $postgresDir, $postgresConf, $postgresData, $postgresAdmin, $postgresBackup -Force
    $postgresConf = Resolve-Path -Path $postgresConf
    Copy-Item -Path "./src/postgres/*" -Destination $postgresConf -Recurse
    Write-Host 'Configuring POSTGRESQL ... Done'
}

# FUNCTION: Configure and run Traefik
function Invoke-Traefik {
    Write-Host 'Configuring TRAEFIK ... for DOCKER'
    $traefikDir = "$baseDir/traefik"
    $traefikConf = "$traefikDir/conf"
    $traefikData = "$traefikDir/data"
    $traefikLogs = "$traefikDir/logs"
    New-Item -ItemType Directory -Path $traefikDir, $traefikConf, $traefikData, $traefikLogs -Force
    $traefikConf = Resolve-Path -Path $traefikConf
    Copy-Item -Path "./src/traefik/*" -Destination $traefikConf -Recurse
    Remove-Item -Path "$traefikLogs/*" -Recurse -Force
    Write-Host 'Configuring TRAEFIK ... Done'
}

#
# START DEVELOPMENT SERVICES
#

# Start development environment
Write-Host 'Starting DEVELOPMENT environment...'

# Check if Docker is available by running the `docker` command
Write-Host 'Validating docker...'
$docker = Get-Docker
if ($docker -ne 'true') {
    throw 'Docker is not installed or active'
}
Set-Docker
Update-Secrets

# Basic paths
Write-Host 'Creating APP directories...'
$baseDir = './.app'
New-Item -ItemType Directory -Path $baseDir -Force

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
Write-Host "Environment variables are:" 
$env:HOSTNAME=$env:COMPUTERNAME
Write-Host " -- HOSTNAME: $env:HOSTNAME" 
Write-Host " -- DOCKER_PUBLIC_IP: $env:DOCKER_PUBLIC_IP" 
Write-Host " -- DOCKER_PRIVATE_IP: $env:DOCKER_PRIVATE_IP" 
if (Test-Path $environmentFile) {
    Get-Content $environmentFile | ForEach-Object {
        $key, $value = $_ -split '='
        [System.Environment]::SetEnvironmentVariable($key.Trim(), $value.Trim(), [System.EnvironmentVariableTarget]::Process)
        Write-Host " -- $key : $value" 
    }
}

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
    "http://config.$env:DOMAIN_DEV",
    "http://s3.$env:DOMAIN_DEV",
    "http://db.$env:DOMAIN_DEV/pgadmin",
    "http://iam.$env:DOMAIN_DEV/",
    "--inprivate",                          # Open in InPrivate mode
    "--ignore-certificate-errors",
    "--ignore-urlfetcher-cert-requests",
    "--start-maximized",                    # Start maximized
    #"--start-minimized",
    "--new-window"                          # Open in a new window

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