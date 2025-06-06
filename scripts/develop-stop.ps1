param (
    [string]$Stack = "app"
)

# Load the required modules
$RootPath = "C:/Users/vince/Sources/huybrechtsxyz"
. "$RootPath/scripts/functions.ps1"

Write-Host "[*] Stopping DEVELOPMENT environment..."

# Loop through all services
if ($Stack -eq "") {
    Get-ChildItem "$AppPath" -Directory | ForEach-Object {
        $ServiceName = $_.Name
        Write-Host "[*] Removing $ServiceName stack..."
        docker stack rm $ServiceName
    }
} else {
    Write-Host "[*] Removing $Stack stack..."
    docker stack rm $Stack
}


Stop-Process -Name 'msedge' -ErrorAction Ignore
Write-Host "[*] Stopping DEVELOPMENT environment...DONE"