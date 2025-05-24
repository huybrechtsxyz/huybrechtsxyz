param (
    [string]$Role,
    [string[]]$Services,
    [bool]$Attach=$true
)

# Run from the root of the repository.
$RootPath = "C:/Users/vince/Sources/huybrechtsxyz"
$SourcePath = "$RootPath/src"
$AppPath = "$RootPath/.app"

# Load the required modules
. "$RootPath/scripts/functions.ps1"

Write-Host "[*] Starting DEVELOPMENT environment..."


Write-Host "[*] Starting DEVELOPMENT environment...DONE"

# Wait for user input to redeploy or stop the environment
if($Attach) {
    $continue = $true
    while ($continue) {
        $key = Read-Host "Press any key to stop debugging or 'r' to redeploy stack"
        
        if ($key -eq 'r' -or $key -eq 'R') {
            Write-Host "Redeploying Docker stack..."
            docker stack deploy -c $composeFile app --detach=true
            Write-Host "Docker stack redeployed with updated compose file."
        }
        elseif ($key -eq '') {
            Write-Host 'Stopping DEVELOPMENT environment...'
            Stop-Process -Name 'msedge' -ErrorAction Ignore
            $continue = $false
        }
    }
}




















# This script runs the local development environment.




# Start development environment
Write-Host 'Starting DEVELOPMENT environment...'

# Basic paths
Write-Host 'Creating APP directories...'
New-Item -ItemType Directory -Path $AppPath -Force
Copy-Item -Path "$SourcePath/develop.env" -Destination "$AppPath/.env" -Force

# Importing the .env file and exporting the variables
$environmentFile = "$SourcePath/develop.env"
Write-Host "Environment variables are:" 
$env:HOSTNAMEID=$(hostname)
$env:HOSTNAME=$env:COMPUTERNAME
$env:DOCKER_MANAGERS=1
Write-Host " -- HOSTNAME: $env:HOSTNAME" 
Write-Host " -- DOCKER_PUBLIC_IP: $env:DOCKER_PUBLIC_IP" 
Write-Host " -- DOCKER_MANGER_COUNT: $env:DOCKER_MANAGER_COUNT" 
if (Test-Path $environmentFile) {
    Get-Content $environmentFile | ForEach-Object {
        $key, $value = $_ -split '='
        [System.Environment]::SetEnvironmentVariable($key.Trim(), $value.Trim(), [System.EnvironmentVariableTarget]::Process)
        Write-Host " -- $key : $value" 
    }
}

# Set the environment variables from the secrets file
if (Test-Path $SecretsPath) {
    $secrets = Get-Content $SecretsPath | ConvertFrom-Json
    foreach ($key in $secrets.PSObject.Properties.Name) {
        $value = $secrets.$key

        if ($value -is [PSCustomObject] -or $value -is [Array]) {
            $value = $value | ConvertTo-Json -Compress
            continue
        }

        [System.Environment]::SetEnvironmentVariable($key, $value, "Process")
        Write-Host "Set environment variable $key : ***"
    }
}
else {
    Write-Host "Secrets file not found at $SecretsPath"
    return
}

# Loop through all service directories
# conf? copy all source files to here
# logs? delete all existing files
Get-ChildItem -Path $SourcePath -Directory | ForEach-Object {
    $serviceDir = $_.FullName
    $serviceName = $_.Name
    $metadataPath = Join-Path $serviceDir "metadata.json"

    Write-Host "Update initialization for $serviceName..."
    if (-Not (Test-Path $metadataPath)) {
        Write-Host "Skipping $serviceName : No metadata.json found." -ForegroundColor Yellow
        return
    }

    try {
        $metadata = Get-Content $metadataPath | ConvertFrom-Json
    } catch {
        Write-Host "Error reading $metadataPath : $_" -ForegroundColor Red
        return
    }

    if ($metadata.servicepaths) {
        foreach ($entry in $metadata.servicepaths) {
            $targetPath = Join-Path $AppPath $entry.path

            if (-Not (Test-Path $targetPath)) {
                Write-Host "Creating directory: $targetPath"
                New-Item -ItemType Directory -Path $targetPath -Force | Out-Null
            }

            if($entry.path -eq "conf") {
                Copy-Item -Path "$SourcePath/$serviceName/*" -Destination $targetPath -Recurse
            }
            
            if($entry.path -eq "logs") {
                Remove-Item -Path "$targetPath/*" -Recurse -Force
            }
        }
    } else {
        Write-Host "No servicepaths found in $metadataPath" -ForegroundColor DarkGray
    }
    Write-Host "Update initialization for $serviceName...DONE"
}

# Check if Docker is available by running the `docker` command
Write-Host 'Validating docker...'
$docker = Test-Docker
if ($docker -ne 'true') {
    throw 'Docker is not installed or active'
}
Enable-Docker

# Configure and run Traefik
Write-Host 'Configuring TRAEFIK ... for DOCKER'
Apply-Template `
    -InputFile "$AppPath/traefik/conf/traefik-config.template.yml" `
    -OutputFile "$AppPath/traefik/conf/traefik-config.yml"
Write-Host 'Configuring TRAEFIK ... Done'

# Configure and run CONSUL
Write-Host 'Configuring CONSUL ... for DOCKER'
Write-Host 'Configuring CONSUL ... Done'

# Configure and run MINIO
Write-Host 'Configuring MINIO ... for DOCKER'
Write-Host 'Configuring MINIO ... Done'

# Configure and run POSTGRES
Write-Host 'Configuring POSTGRES ... for DOCKER'
Add-DockerNodeLabel -NodeName "docker-desktop" -LabelKey "postgres"
Write-Host 'Configuring POSTGRES ... Done'

# Configure and run KEYCLOAK
Write-Host 'Configuring KEYCLOAK ... for DOCKER'
Merge-Template `
    -InputFile "$APPPATH/keycloak/conf/keycloak-realm.template.json" `
    -OutputFile "$APPPATH/keycloak/conf/keycloak-realm.json"
Write-Host 'Configuring KEYCLOAK ... Done'

# Configure and run TELEMETRY
Write-Host 'Configuring TELEMETRY ... for DOCKER'
Write-Host 'Configuring TELEMETRY ... Done'

# Loop through all services
Get-ChildItem "$AppPath\src" -Directory | ForEach-Object {
    $ServicePath = $_.FullName
    $MetadataFile = Join-Path $ServicePath "metadata.json"
    $ComposeFile = Join-Path $ServicePath "compose.yml"

    if (-not (Test-Path $MetadataFile) -or -not (Test-Path $ComposeFile)) {
        return
    }

    $meta = Get-Content $MetadataFile | ConvertFrom-Json
    $serviceName = $meta.service
    $roleName = $meta.role

    if ($Services.Count -gt 0 -and $Services -notcontains $serviceName) {
        return
    }

    Write-Host "Validating Docker Compose for $serviceName..."
    docker compose -f $composeFile --env-file $environmentFile config
    Write-Host "Deploying $ServiceName stack..."
    docker stack deploy -c $ComposeFile $ServiceName --detach=true
}

# DEBUG AND TEST
Start-Process -FilePath "msedge.exe" `
    "http://proxy.$env:DOMAIN_DEV/dashboard/",
    "http://config.$env:DOMAIN_DEV/",
    "http://iam.$env:DOMAIN_DEV",
    "http://s3.$env:DOMAIN_DEV",
    "http://db.$env:DOMAIN_DEV/pgamin",
    "--inprivate",
    "--ignore-certificate-errors",
    "--ignore-urlfetcher-cert-requests",
    "--start-maximized",
    #"--start-minimized",
    "--new-window"



# Stop Docker Compose
Write-Host "Stopping Docker Swarm..."
#docker compose -f $composeFile down
docker stack rm app

# Stop the development environment
Write-Host 'DEVELOPMENT environment stopped.'
