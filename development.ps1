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

# Function Configure and run Consul
function Invoke-Consul {
    Write-Host 'Configuring CONSUL ...'
    $consulDir = "$baseDir/consul"
    $consulConf = "$consulDir/conf"
    $consulData = "$consulDir/data"
    New-Item -ItemType Directory -Path $consulDir, $consulConf, $consulData -Force
    $consulDir = Resolve-Path -Path $consulDir
    $consulExe = "$consulDir/consul.exe"
    Copy-Item -Path "./src/consul/*" -Destination $consulConf -Recurse
    
    if ($docker -eq 'true') {
        Write-Host 'Configuring CONSUL ... for DOCKER'
    } else {
        Write-Host 'Configuring CONSUL ... for LOCALHOST'
        if (-Not (Test-Path -Path $consulExe)) {
            Write-Output " -- Extracting Consul..."
            Extract-ZipFile -zipFile "./zips/consul_1.20.1.zip" -extractPath $consulDir
        }
        Write-Host " -- Setting environment ..."
        $env:CONSUL_ADDR="http://127.0.0.1:8500"
        Write-Host " -- Starting executable ..."
        Start-Process -FilePath $consulExe -ArgumentList "agent", "-dev", "-config-dir $consulData" -WindowStyle Minimized
    }
    Write-Host 'Configuring CONSUL ... done'
}

# Function Configure and run minio
function Invoke-Minio {
    Write-Host 'Configuring MINIO ...'
    $minioDir = "$baseDir/minio"
    #$minioConf = "$minioDir/conf"
    $minioData = "$minioDir/data"
    New-Item -ItemType Directory -Path $minioDir, $minioData -Force
    $minioDir = Resolve-Path -Path $minioDir
    $minioExe = "$minioDir/minio.exe"
    #Copy-Item -Path "./src/minio/*" -Destination $minioConf -Recurse
    Write-Host " -- Setting environment ..."
    $env:MINIO_ROOT_USER="admin"
    $env:MINIO_ROOT_PASSWORD="password"

    if ($docker -eq 'true') {
        Write-Host 'Configuring MINIO ... for DOCKER'
    } else {
        Write-Host 'Configuring MINIO ... for LOCALHOST'
        if (-Not (Test-Path -Path $minioExe)) {
            Write-Output " -- Extracting minio..."
            Expand-ZipFile -zipFile "./zips/minio-v202411.zip" -extractPath $minioDir
        }
        Write-Host " -- Starting executable ..."
        Start-Process -FilePath $minioExe -ArgumentList 'server /data --console-address ":9001"' -WindowStyle Minimized
    }
    Write-Host 'Configuring MINIO ... done'
}

# Function Configure and run Prometheus
function Invoke-Prometheus {
    Write-Host 'Configuring PROMETHEUS ...'
    $prometheusDir = "$baseDir/prometheus"
    $prometheusConf = "$prometheusDir/conf"
    $prometheusData = "$prometheusDir/data"
    New-Item -ItemType Directory -Path $prometheusDir, $prometheusConf, $prometheusData -Force
    Copy-Item -Path "./src/prometheus/*" -Destination $prometheusConf -Recurse
    if ($docker -eq 'true') {
        Write-Host 'Configuring PROMETHEUS ... for DOCKER'
    } else {
        Write-Host 'Configuring PROMETHEUS ... skipping'
    }
    Write-Host 'Configuring PROMETHEUS ... done'
}

# Function Configure and run thanos
function Invoke-Thanos {
    Write-Host 'Configuring THANOS ...'
    $thanosDir = "$baseDir/thanos"
    $thanosConf = "$thanosDir/conf"
    $thanosData = "$thanosDir/data"
    New-Item -ItemType Directory -Path $thanosDir, $thanosConf, $thanosData -Force
    Copy-Item -Path "./src/thanos/*" -Destination $thanosConf -Recurse
    if ($docker -eq 'true') {
        Write-Host 'Configuring THANOS ... for DOCKER'
    } else {
        Write-Host 'Configuring THANOS ... skipping'
    }
    Write-Host 'Configuring THANOS ... done'
}

# Function Configure and run Traefik
function Invoke-Traefik {
    Write-Host 'Configuring TRAEFIK ...'
    $traefikDir = "$baseDir/traefik"
    $traefikCert = "$traefikDir/cert"
    $traefikLogs = "$traefikDir/logs"
    New-Item -ItemType Directory -Path $traefikDir, $traefikCert, $traefikLogs -Force
    if ($docker -eq 'true') {
        Write-Host 'Configuring TRAEFIK ... for DOCKER'
    } else {
        Write-Host 'Configuring TRAEFIK ... skipping'
    }
    Write-Host 'Configuring TRAEFIK ... done'
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
New-Item -ItemType Directory -Path $baseDir -Force

# Configure and run services
Invoke-Consul
Invoke-Minio
Invoke-Traefik
Invoke-Prometheus
Invoke-Thanos

# Debug and test
if ($docker -eq 'true') {
    #
    # USE DOCKER
    #
    $composeFile = "./src/compose.develop.yml"
    Write-Host "Starting Docker Compose..."
    docker-compose -f $composeFile config
    docker-compose -f $composeFile up -d

    # DEBUG AND TEST
    Start-Process -FilePath "msedge.exe" -ArgumentList `
        "http://proxy.localhost/dashboard/#/",  # Traefik dashboard
        "http://localhost:8500",                # Consul dashboard
        "http://localhost:9090",                # Prometheus dashboard
        "http://localhost:9001",                # Minio dashboard
        "--inprivate",                          # Open in InPrivate mode
        "--start-maximized",                    # Start maximized
        "--new-window"                          # Open in a new window
} else {
    #
    # USE EXECUTABLES
    #
    Start-Process -FilePath "msedge.exe" -ArgumentList `
        "http://localhost:8500",                # Consul
        "http://localhost:9001",                # Minio dashboard
        "--inprivate",                          # Open in InPrivate mode
        "--start-maximized",                    # Start maximized
        "--new-window"                          # Open in a new window
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
    Stop-Process -Name 'minio' -ErrorAction Ignore
}

# Stop the development environment
Write-Output 'DEVELOPMENT environment stopped.'