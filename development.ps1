# FUNCTION: EXTRACT
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

# Start development engine
Write-Output 'Starting DEVELOPMENT environment...'

# Check if Docker is available by running the `docker` command
try {
    # Try running 'docker --version' to check if Docker is installed and available
    $dockerVersion = & docker --version 2>&1

    # If Docker is available, $dockerVersion should contain version information, otherwise it throws an error
    if ($dockerVersion -match "Docker version") {
        Write-Output " -- Docker is available"
        
        # Check if Docker is running
        try {
            $output = docker run hello-world 2>&1
            if ($output -like "*docker: error during connect*") {
                Write-Host " -- Docker is not running"
            } else {
                Write-Host " -- Docker is running"
                $docker = 'true'
            }
        } catch {
            Write-Host " -- Docker is not running."
        }
        
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
        "http://localhost:80/dashboard/",    # Dashboard URL
        #"http://localhost:8500",             # Config URL
        #"http://localhost:8180",             # PgAdmin URL
        #"http://localhost:9001",             # Data URL
        #"http://localhost:8200",             # Documentation URL
        "--inprivate",                       # Open in InPrivate mode
        "--start-maximized",                 # Start maximized
        "--new-window"                       # Open in a new window
}
else
{
    # Start-Process -FilePath "msedge.exe" -ArgumentList `
    #     "http://localhost:8500",             # Config URL
    #     "http://localhost:9001",             # Data URL
    #     "--inprivate",                       # Open in InPrivate mode
    #     "--start-maximized",                 # Start maximized
    #     "--new-window"                       # Open in a new window
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
}

# SYSTEM
Write-Output 'DEVELOPMENT environment stopped.'