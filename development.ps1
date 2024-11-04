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

# SYSTEM
Write-Output 'Starting DEVELOPMENT environment...'

# BASIC PATHS
$baseDir = "./.app"
$certDir = "$baseDir/cert"
$dataDir = "$baseDir/data"
$logsDir = "$baseDir/logs"
New-Item -ItemType Directory -Path $baseDir, $certDir, $dataDir, $logsDir -Force

# CONSUL
Write-Output "Starting Consul..."
$consulDir = Resolve-Path -Path "$baseDir/consul"
$consulExe = "$consulDir/consul.exe"
$consulData = Resolve-Path -Path "$dataDir/consul"
New-Item -ItemType Directory -Path $consulDir, $consulData -Force

if (-Not (Test-Path -Path $consulExe)) {
    Write-Output "    Extracting Consul..."
    Extract-ZipFile -zipFile "./zips/consul_1.20.1.zip" -extractPath $consulDir
}

Write-Output "    Configure Consul..."
$env:CONSUL_ADDR="http://127.0.0.1:8500"
Write-Output "    Start Consul..."
Start-Process -FilePath "$consulDir/consul.exe" -ArgumentList "agent", "-dev" -WindowStyle Minimized

# MINIO
Write-Output "Starting Minio..."
$minioDir = Resolve-Path -Path "$baseDir/minio"
$minioExe = "$minioDir/minio.exe"
$minioData = Resolve-Path -Path "$dataDir/minio"
New-Item -ItemType Directory -Path $minioDir, $minioData -Force

if (-Not (Test-Path -Path $minioExe)) {
    Write-Output "Extracting Minio..."
    Extract-ZipFile -zipFile "./zips/minio-v202411.zip" -extractPath $minioDir
}

$env:MINIO_ROOT_USER="admin"
$env:MINIO_ROOT_PASSWORD="password"
Start-Process -FilePath $minioExe -ArgumentList "server $minioData --console-address `":9001`"" 

# DEBUG AND TEST
Start-Process -FilePath "msedge.exe" -ArgumentList `
   "http://localhost:8500 http://localhost:9001",`
   "--incognito --start-maximized --new-window"

Pause 'Press any key to stop debugging'
Write-Output 'Stopping Local Host environment...'

# STOPPING SERVICES
Stop-Process -Name 'msedge' -ErrorAction Ignore
Stop-Process -Name 'consul' -ErrorAction Ignore
Stop-Process -Name 'minio' -ErrorAction Ignore

# SYSTEM
Write-Output 'Local Host environment stopped.'