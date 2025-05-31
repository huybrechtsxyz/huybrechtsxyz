param (
    [string]$Group,
    [string[]]$Services
)

# Load the required modules
$RootPath = "C:/Users/vince/Sources/huybrechtsxyz"
. "$RootPath/scripts/functions.ps1"

Set-Location $AppPath

Write-Host "[*] Starting DEVELOPMENT environment..."

# Basic paths
Write-Host '[*] Creating APP directories...'
New-Item -ItemType Directory -Path $AppPath -Force
Copy-Item -Path "$SourcePath/develop.env" -Destination "$AppPath/.env" -Force

# Importing the .env file and exporting the variables
$environmentFile = "$SourcePath/develop.env"
Write-Host "[*] Environment variables are:" 
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
Write-Host "[*] Setting secret variables:" 
if (Test-Path $SecretsPath) {
    $secrets = Get-Content $SecretsPath | ConvertFrom-Json
    foreach ($key in $secrets.PSObject.Properties.Name) {
        $value = $secrets.$key

        if ($value -is [PSCustomObject] -or $value -is [Array]) {
            $value = $value | ConvertTo-Json -Compress
            continue
        }

        [System.Environment]::SetEnvironmentVariable($key, $value, "Process")
        Write-Host "  - Set environment variable $key : ***"
    }
}
else {
    Write-Host "  - Secrets file not found at $SecretsPath"
    return
}

# Loop through all service directories
# conf? copy all source files to here
# logs? delete all existing files
Get-ChildItem -Path $SourcePath -Directory | ForEach-Object {
    $serviceDir = $_.FullName
    $serviceName = $_.Name
    $metadataPath = Join-Path $serviceDir "metadata.json"
    $servicePath = Join-Path -Path $AppPath -ChildPath $_.Name

    Write-Host "[*] Update initialization for $serviceName..."
    if (-Not (Test-Path $metadataPath)) {
        Write-Host "  - Skipping $serviceName : No metadata.json found." -ForegroundColor Yellow
        return
    }

    try {
        $metadata = Get-Content $metadataPath | ConvertFrom-Json
    } catch {
        Write-Host " - Error reading $metadataPath : $_" -ForegroundColor Red
        return
    }

    if ($metadata.servicepaths) {
        foreach ($entry in $metadata.servicepaths) {
            $targetPath = Join-Path -Path $servicePath -ChildPath $entry.path

            if (-Not (Test-Path $targetPath)) {
                Write-Host " - Creating directory: $targetPath"
                New-Item -ItemType Directory -Path $targetPath -Force | Out-Null
            }

            if($entry.path -eq "conf") {
                Write-Host " - Copying configuration files to $targetPath"
                Copy-Item -Path "$SourcePath/$serviceName/*" -Destination $targetPath -Recurse
            }
            
            if($entry.path -eq "logs") {
                Write-Host " - Clearing logs"
                Remove-Item -Path "$targetPath/*" -Recurse -Force
            }
        }
    } else {
        Write-Host "  - No servicepaths found in $metadataPath" -ForegroundColor DarkGray
    }
    Write-Host "  - Update initialization for $serviceName...DONE"
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
if(Test-Path -Path "$AppPath/traefik/conf/traefik-config.template.yml") {
    Merge-Template `
        -InputFile "$AppPath/traefik/conf/traefik-config.template.yml" `
        -OutputFile "$AppPath/traefik/conf/traefik-config.yml"
    Move-Item -Path "$AppPath/traefik/conf/consul.traefik.json" -Destination "$AppPath/consul/etc" -Force
}
Write-Host 'Configuring TRAEFIK ... Done'

# Configure and run CONSUL
Write-Host 'Configuring CONSUL ... for DOCKER'
Move-Item -Path "$AppPath/consul/conf/etc/*" -Destination "$AppPath/consul/etc" -Force
Remove-Item -Path "$AppPath/consul/conf/etc" -Force
Write-Host 'Configuring CONSUL ... Done'

# Configure and run MINIO
Write-Host 'Configuring MINIO ... for DOCKER'
Move-Item -Path "$AppPath/minio/conf/consul.minio.json" -Destination "$AppPath/consul/etc" -Force
Write-Host 'Configuring MINIO ... Done'

# Configure and run POSTGRES
Write-Host 'Configuring POSTGRES ... for DOCKER'
Move-Item -Path "$AppPath/postgres/conf/consul.postgres.json" -Destination "$AppPath/consul/etc" -Force
Write-Host 'Configuring POSTGRES ... Done'

# Configure and run REDIS
Write-Host 'Configuring REDIS ... for DOCKER'
Move-Item -Path "$AppPath/redis/conf/consul.redis.json" -Destination "$AppPath/consul/etc" -Force
Write-Host 'Configuring REDIS ... Done'

# Configure and run KEYCLOAK
# Write-Host 'Configuring KEYCLOAK ... for DOCKER'
# if (Test-Path -Path "$AppPath/keycloak/conf/keycloak-realm.template.json") {
#     Merge-Template `
#         -InputFile "$AppPath/keycloak/conf/keycloak-realm.template.json" `
#         -OutputFile "$AppPath/keycloak/conf/keycloak-realm.json"
#     Move-Item -Path "$AppPath/keycloak/conf/consul.keycloak.json" -Destination "$AppPath/consul/etc" -Force
# }
# Write-Host 'Configuring KEYCLOAK ... Done'

# Configure and run TELEMETRY
# Write-Host 'Configuring TELEMETRY ... for DOCKER'
# Write-Host 'Configuring TELEMETRY ... Done'

# Loop through all services
Get-ChildItem "$AppPath" -Directory | ForEach-Object {
    $ServicePath = $_.FullName
    $MetadataFile = Join-Path $ServicePath "conf/metadata.json"
    $ComposeFile = Join-Path $ServicePath "conf/compose.yml"

    if (-not (Test-Path $MetadataFile) -or -not (Test-Path $ComposeFile)) {
        return
    }

    $meta = Get-Content $MetadataFile | ConvertFrom-Json
    $serviceName = $meta.service
    $groupName = $meta.group

    if ($Services.Count -gt 0 -and $Services -notcontains $serviceName -and $Group -ne $groupName) {
        Write-Host "Skipping $serviceName..."
        return
    }

    #Write-Host "Validating Docker Compose for $serviceName..."
    #docker compose -f $composeFile --env-file $environmentFile config
    Write-Host "Deploying $ServiceName stack..."
    docker stack deploy -c $ComposeFile $ServiceName --detach=true
}

# DEBUG AND TEST
Start-Process -FilePath "msedge.exe" `
    "http://proxy.$env:DOMAIN_DEV/dashboard/",
    "http://cfg.$env:DOMAIN_DEV/",
    "http://s3.$env:DOMAIN_DEV",
    "http://db.$env:DOMAIN_DEV/pgadmin",
    "http://redis.$env:DOMAIN_DEV",
    "--inprivate",
    "--ignore-certificate-errors",
    "--ignore-urlfetcher-cert-requests",
    "--start-maximized",
    #"--start-minimized",
    "--new-window"

Write-Host "[*] Starting DEVELOPMENT environment...DONE"