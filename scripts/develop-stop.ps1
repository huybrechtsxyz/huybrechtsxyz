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
    $ServicePath = $_.FullName
    $MetadataFile = Join-Path $ServicePath "metadata.json"
    $ComposeFile = Join-Path $ServicePath "compose.yml"

    if (-not (Test-Path $MetadataFile) -or -not (Test-Path $ComposeFile)) {
        return
    }

    $meta = Get-Content $MetadataFile | ConvertFrom-Json
    $serviceName = $meta.service
    $groupName = $meta.group

    if ($Services.Count -gt 0 -and $Services -notcontains $serviceName -and $Group -ne $groupName) {
        return
    }

    Write-Host "Validating Docker Compose for $serviceName..."
    docker compose -f $composeFile --env-file $environmentFile config
    Write-Host "Deploying $ServiceName stack..."
    docker stack deploy -c $ComposeFile $ServiceName --detach=true
}

Write-Host "[*] Stopping DEVELOPMENT environment...DONE"