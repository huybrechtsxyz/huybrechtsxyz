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
        Write-Output "Zip file extracted successfully to $extractPath"
    } else {
        Write-Output "Zip file not found at $zipFile"
    }
}

# FUNCTION: Check if Docker is available by running the `docker` command
function Get-Docker {
    try {
        # Try running 'docker --version' to check if Docker is installed and available
        $dockerVersion = & docker --version 2>&1
        # If Docker is available, $dockerVersion should contain version information, otherwise it throws an error
        if ($dockerVersion -match "Docker version") {
            Write-Output " -- Docker is available"
            # Check if Docker is running
            try {
                $output = docker run --rm hello-world 2>&1
                if ($output -like "*docker: error during connect*") {
                    Write-Output " -- Docker is not running"
                    return $false
                } else {
                    Write-Output " -- Docker is running"
                    return $true
                }
            } catch {
                Write-Output " -- Docker is not running."
                return $false
            }
        } else {
            Write-Output " -- Docker not found"
            return $false
        }
    } catch {
        # Handle the case where Docker isn't available
        Write-Output " -- Docker is not installed."
        return $false
    }
}

# FUNCTION: Configure required docker networks
function Set-Docker {
    # Check and create the "traefik" network if it doesn't exist
    $traefikNetwork = "traefik"
    if (!(docker network ls --format '{{.Name}}' | Select-String -Pattern "^$traefikNetwork$")) {
        Write-Host "Creating external network '$traefikNetwork'..."
        docker network create --driver=bridge --attachable $traefikNetwork
    } else {
        Write-Host "Network '$traefikNetwork' already exists."
    }

    # Check and create the "intranet" network if it doesn't exist
    $intranetNetwork = "intranet"
    if (!(docker network ls --format '{{.Name}}' | Select-String -Pattern "^$intranetNetwork$")) {
        Write-Host "Creating bridge network '$intranetNetwork'..."
        docker network create --driver=bridge $intranetNetwork
    } else {
        Write-Host "Network '$intranetNetwork' already exists."
    }
}

# FUNCTION: Configure and run consul
function Invoke-Consul {
    if ($docker -eq 'true')
    {
        Write-Output "Configuring CONSUL for Docker..."
        $consulData = "$dataDir/consul"
        New-Item -ItemType Directory -Path $consulData -Force
        Copy-Item -Path "./src/consul/*" -Destination $baseDir -Recurse
    }
    else
    {
        Write-Output "Configuring CONSUL for Executable..."
        $consulDir = "$baseDir/consul"
        $consulData = "$dataDir/consul"
        New-Item -ItemType Directory -Path $consulDir, $consulData -Force
        $consulDir = Resolve-Path -Path $consulDir
        $consulData = Resolve-Path -Path $consulData
        $consulExe = "$consulDir/consul.exe"

        Copy-Item -Path "./src/consul/*" -Destination $consulData -Recurse

        if (-Not (Test-Path -Path $consulExe)) {
            Write-Output " -- Extracting Consul..."
            Expand-ZipFile -zipFile "./zips/consul_1.20.1.zip" -extractPath $consulDir
        }
    
        Write-Output " -- Setting environment ..."
        $env:CONSUL_ADDR="http://127.0.0.1:8500"
        Write-Output " -- Starting executable ..."
        Start-Process -FilePath $consulExe -ArgumentList "agent", "-dev", "-config-dir $consulData" -WindowStyle Minimized
    }
}

# FUNCTION: Configure and run prometheus
function Invoke-Prometheus {
    if ($docker -eq 'true')
    {
        Write-Output "Configuring PROMETHEUS for Docker..."
        $prometheusData = "$dataDir/prometheus"
        New-Item -ItemType Directory -Path $prometheusData -Force
        Copy-Item -Path "./src/prometheus/*" -Destination $baseDir -Recurse
    }
    else
    {
        Write-Output "Skipping PROMETHEUS for Executable..."
    }
}

#
# START DEVELOPMENT SERVICES
#

# Start development environment
Write-Output 'Starting DEVELOPMENT environment...'

# Check if Docker is available by running the `docker` command
Write-Output 'Validating docker...'
$docker = Get-Docker
if ($docker -eq 'true') {
    Set-Docker
}

# Basic paths
Write-Output 'Creating APP directories...'
$baseDir = "./.app"
$certDir = "$baseDir/cert"
$dataDir = "$baseDir/data"
$logsDir = "$baseDir/logs"
New-Item -ItemType Directory -Path $baseDir, $certDir, $dataDir, $logsDir -Force

# Configure and run keycloak
Invoke-Consul
Invoke-Prometheus

if ($docker -eq 'true') {
    #
    # USE DOCKER
    #
    $composeFile = "./src/compose.develop.yml"
    Write-Host "Starting Docker Compose..."
    docker-compose -f $composeFile up -d

    # DEBUG AND TEST
    Start-Process -FilePath "msedge.exe" -ArgumentList `
        "http://consul.localhost:8500",         # Consul
        "http://proxy.localhost/dashboard/#/",  # Traefik dashboard
        "http://prometheus.localhost:9090",     # Prometheus dashboard
        "--inprivate",                          # Open in InPrivate mode
        "--start-maximized",                    # Start maximized
        "--new-window"                          # Open in a new window
} else {
    #
    # USE EXECUTABLES
    #
    Start-Process -FilePath "msedge.exe" -ArgumentList `
        "http://localhost:8500",             # Consul
        "--inprivate",                       # Open in InPrivate mode
        "--start-maximized",                 # Start maximized
        "--new-window"                       # Open in a new window
}

# Stopping the development environment
Pause 'Press any key to stop debugging'
Write-Output 'Stopping DEVELOPMENT environment...'
Stop-Process -Name 'msedge' -ErrorAction Ignore

if ($docker -eq 'true') {
    # Stop Docker Compose
    Write-Host "Stopping Docker Compose..."
    docker-compose -f $composeFile down
} else {
    # STOPPING SERVICES
    Stop-Process -Name 'msedge' -ErrorAction Ignore
    Stop-Process -Name 'consul' -ErrorAction Ignore
}

# Stop the development environment
Write-Output 'DEVELOPMENT environment stopped.'