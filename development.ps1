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
Write-Output 'Starting Local Host environment...'

# VARIABLES
$baseDir = "./.app"
$certDir = "$baseDir/cert"
$dataDir = "$baseDir/data"
$consulDir = "$baseDir/consul"

# APPDIR
New-Item -ItemType Directory -Path $baseDir, $certDir, $dataDir, $consulDir -Force

# CONSUL EXISTS?
if (Test-Path -Path "./app/consul/consul.exe") {
    Extract-ZipFile -zipFile "./zips/consul_1.20.1.zip" -extractPath $consulDir
}

# CONSUL SERVER/AGENT
$env:CONSUL_ADDR="http://127.0.0.1:8500"
Start-Process -FilePath ".app/consul/consul.exe" -ArgumentList "agent", "-dev" -WindowStyle Minimized

# DEBUG AND TEST

Start-Process -FilePath "msedge.exe" -ArgumentList `
   "http://localhost:8500 ",`
   "--incognito --start-maximized --new-window"

Pause 'Press any key to stop debugging'
Write-Output 'Stopping Local Host environment...'

# STOPPING SERVICES
Stop-Process -Name 'msedge' -ErrorAction Ignore
Stop-Process -Name 'consul' -ErrorAction Ignore

# SYSTEM
Write-Output 'Local Host environment stopped.'