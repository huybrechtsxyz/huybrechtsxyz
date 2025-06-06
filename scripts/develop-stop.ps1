param (
    [string]$Role,
    [string[]]$Services
)

# Load the required modules
$RootPath = "C:/Users/vince/Sources/huybrechtsxyz"
. "$RootPath/scripts/functions.ps1"

Write-Host "[*] Stopping DEVELOPMENT environment..."

# Loop through all services
Get-ChildItem "$AppPath" -Directory | ForEach-Object {
    $ServiceName = $_.Name
    Write-Host "Removing $ServiceName stack..."
    docker stack rm $ServiceName
}

Stop-Process -Name 'msedge' -ErrorAction Ignore
Write-Host "[*] Stopping DEVELOPMENT environment...DONE"