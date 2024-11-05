# SYSTEM
Write-Output 'Starting DEVELOPMENT environment...'

# BASIC PATHS
$baseDir = "./.app"
$certDir = "$baseDir/cert"
$dataDir = "$baseDir/data"
$logsDir = "$baseDir/logs"
New-Item -ItemType Directory -Path $baseDir, $certDir, $dataDir, $logsDir -Force

# Start Docker Compose
$composeFile = "./src/compose.development.yml"
Write-Host "Starting Docker Compose..."
docker-compose -f $composeFile up -d

# DEBUG AND TEST
Start-Process -FilePath "msedge.exe" -ArgumentList `
   "http://localhost:8500 http://localhost:9001", `
   "--inprivate", "--start-maximized", "--new-window"

Pause 'Press any key to stop debugging'
Write-Output 'Stopping DEVELOPMENT environment...'

# Stop Docker Compose
Write-Host "Stopping Docker Compose..."
docker-compose -f $composeFile down

# SYSTEM
Write-Output 'DEVELOPMENT environment stopped.'