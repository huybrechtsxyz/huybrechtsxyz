# Start development engine
Write-Output 'Starting DEVELOPMENT environment...'

# Basic paths
$baseDir = "./.app"
$certDir = "$baseDir/cert"
$dataDir = "$baseDir/data"
$logsDir = "$baseDir/logs"
New-Item -ItemType Directory -Path $baseDir, $certDir, $dataDir, $logsDir -Force

# Consul paths
$consulData = "$dataDir/consul"
New-Item -ItemType Directory -Path $consulData -Force
Copy-Item -Path "./src/consul/*" -Destination $baseDir -Recurse

# Minio paths
$minioData = "$dataDir/minio"
New-Item -ItemType Directory -Path $minioData -Force

# Documentation path
$docsData = "$baseDir/docs"
New-Item -ItemType Directory -Path $docsData -Force
Copy-Item -Path "./src/mkdocs/mkdocs.yml" -Destination "$baseDir/mkdocs.yml"
Copy-Item -Path "./docs/*" -Destination $docsData -Recurse -Force

# Define network names
$traefikNetwork = "traefik"
$intranetNetwork = "intranet"

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
   "http://proxy.localhost/dashboard/ http://config.localhost http://pgadmin.localhost:8080 http://data.localhost:9001 http://docs.localhost:8000", `
   "--inprivate", "--start-maximized", "--new-window"

Pause 'Press any key to stop debugging'
Write-Output 'Stopping DEVELOPMENT environment...'
Stop-Process -Name 'msedge' -ErrorAction Ignore

# Stop Docker Compose
Write-Host "Stopping Docker Compose..."
docker-compose -f $composeFile down

# SYSTEM
Write-Output 'DEVELOPMENT environment stopped.'