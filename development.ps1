# FUNCTION: EXTRACT
function Extract-ZipFile {
    param (
        [string]$zipFile,
        [string]$extractPath
    )

    # Check if the zip file exists
    if (Test-Path -Path $zipFile) {
        # Load the compression assembly and extract the zip file
        Add-Type -AssemblyName System.IO.Compression.FileSystem
        [System.IO.Compression.ZipFile]::ExtractToDirectory($zipFile, $extractPath)
        Write-Output "Zip file extracted successfully to $extractPath"
    } else {
        Write-Output "Zip file not found at $zipFile"
    }
}

# Start development engine
Write-Output 'Starting DEVELOPMENT environment...'

# Set with or without docker 
$docker = 'false'

# Check if Docker is available by running the `docker` command
try {
    # Try running 'docker --version' to check if Docker is installed and available
    $dockerVersion = & docker --version 2>&1

    # If Docker is available, $dockerVersion should contain version information, otherwise it throws an error
    if ($dockerVersion -match "Docker version") {
        Write-Output " -- Docker is available"
        
        # Run docker-compose if Docker is available
        $docker = 'true'
    } else {
        throw " -- Docker not found"
    }
}
catch {
    # Handle the case where Docker isn't available
    Write-Output " -- Docker is not installed."
}

# Basic paths
Write-Output 'Creating APP directories...'
$baseDir = "./.app"
$certDir = "$baseDir/cert"
$dataDir = "$baseDir/data"
$logsDir = "$baseDir/logs"
New-Item -ItemType Directory -Path $baseDir, $certDir, $dataDir, $logsDir -Force

# CONSUL
if ($docker -eq 'true')
{
    Write-Output "Configurating CONSUL for Docker..."
    $consulData = "$dataDir/consul"
    New-Item -ItemType Directory -Path $consulData -Force
    Copy-Item -Path "./src/consul/*" -Destination $dataDir -Recurse
}
else
{
    Write-Output "Configurating CONSUL for Executable..."
    $consulDir = "$baseDir/consul"
    $consulData = "$dataDir/consul"
    New-Item -ItemType Directory -Path $consulDir, $consulData -Force
    $consulDir = Resolve-Path -Path $consulDir
    $consulData = Resolve-Path -Path $consulData
    $consulExe = "$consulDir/consul.exe"

    Copy-Item -Path "./src/consul/*" -Destination $consulData -Recurse

    if (-Not (Test-Path -Path $consulExe)) {
        Write-Output " -- Extracting Consul..."
        Extract-ZipFile -zipFile "./zips/consul_1.20.1.zip" -extractPath $consulDir
    }

    Write-Output " -- Setting environment ..."
    $env:CONSUL_ADDR="http://127.0.0.1:8500"
    Write-Output " -- Starting executable ..."
    Start-Process -FilePath $consulExe -ArgumentList "agent", "-dev", "-config-dir $consulData" -WindowStyle Minimized
}

# MINIO
if ($docker -eq 'true')
{
    Write-Output "Configurating MINIO for Docker..."
    $minioData = "$dataDir/minio"
    New-Item -ItemType Directory -Path $minioData -Force
}
else
{
    Write-Output "Configurating MINIO for Executable..."
    $minioDir = "$baseDir/minio"
    $minioData = "$dataDir/minio"
    New-Item -ItemType Directory -Path $minioDir, $minioData -Force
    $minioDir = Resolve-Path -Path $minioDir
    $minioData = Resolve-Path -Path $minioData
    $minioExe = "$minioDir/minio.exe"

    if (-Not (Test-Path -Path $minioExe)) {
        Write-Output " -- Extracting Minio..."
        Extract-ZipFile -zipFile "./zips/minio-v202411.zip" -extractPath $minioDir
    }

    Write-Output " -- Setting environment ..."
    $env:MINIO_ROOT_USER="admin"
    $env:MINIO_ROOT_PASSWORD="password"
    Write-Output " -- Starting executable ..."
    Start-Process -FilePath $minioExe -ArgumentList "server $minioData --console-address `":9001`"" -WindowStyle Minimized
}

# Documentation path
$docsData = "$baseDir/docs"
New-Item -ItemType Directory -Path $docsData -Force
Copy-Item -Path "./src/mkdocs/mkdocs.yml" -Destination "$baseDir/mkdocs.yml"
Copy-Item -Path "./docs/*" -Destination $docsData -Recurse -Force

# Elastic search
$elasticData = "$dataDir/elastic"
New-Item -ItemType Directory -Path $elasticData -Force
Copy-Item -Path "./src/elastic/*" -Destination $dataDir

# Define network names
$traefikNetwork = "traefik"
$intranetNetwork = "intranet"

if ($docker -eq 'true')
{
    # Check and create the "traefik" network if it doesn't exist
    if (!(docker network ls --format '{{.Name}}' | Select-String -Pattern "^$traefikNetwork$")) {
        Write-Host "Creating external network '$traefikNetwork'..."
        docker network create --driver=bridge --attachable $traefikNetwork
    } else {
        Write-Host "Network '$traefikNetwork' already exists."
    }

    # Check and create the "intranet" network if it doesn't exist
    if (!(docker network ls --format '{{.Name}}' | Select-String -Pattern "^$intranetNetwork$")) {
        Write-Host "Creating bridge network '$intranetNetwork'..."
        docker network create --driver=bridge $intranetNetwork
    } else {
        Write-Host "Network '$intranetNetwork' already exists."
    }

    # Start docker compose
    $composeFile = "./src/compose.develop.yml"
    Write-Host "Starting Docker Compose..."
    docker-compose -f $composeFile up -d

    # DEBUG AND TEST
    Start-Process -FilePath "msedge.exe" -ArgumentList `
        "http://proxy.localhost/dashboard/",     # Dashboard URL
        "http://config.localhost",               # Config URL
        "http://pgadmin.localhost:8080",         # PgAdmin URL
        "http://data.localhost:9001",            # Data URL
        "http://docs.localhost:8000",            # Documentation URL
        "--inprivate",                           # Open in InPrivate mode
        "--start-maximized",                     # Start maximized
        "--new-window"                           # Open in a new window
}
else
{
    Start-Process -FilePath "msedge.exe" -ArgumentList `
        "http://localhost:8500",             # Config URL
        "http://localhost:9001",             # Data URL
        "--inprivate",                       # Open in InPrivate mode
        "--start-maximized",                 # Start maximized
        "--new-window"                       # Open in a new window
}

Pause 'Press any key to stop debugging'
Write-Output 'Stopping DEVELOPMENT environment...'
Stop-Process -Name 'msedge' -ErrorAction Ignore

if ($docker -eq 'true')
{
    # Stop Docker Compose
    Write-Host "Stopping Docker Compose..."
    docker-compose -f $composeFile down
}
else
{
    # STOPPING SERVICES
    Stop-Process -Name 'msedge' -ErrorAction Ignore
    Stop-Process -Name 'consul' -ErrorAction Ignore
    Stop-Process -Name 'minio' -ErrorAction Ignore
}

# SYSTEM
Write-Output 'DEVELOPMENT environment stopped.'