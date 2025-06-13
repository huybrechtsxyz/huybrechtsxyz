param (
    [string]$Stack = "app",
    [string]$Group = "",
    [string[]]$Services = @()
)

# Load the required modules
. "$env:USERPROFILE/Sources/huybrechtsxyz/scripts/functions.ps1"
# RootPath from functions
# AppPath from functions
# SourcePath from functions

Set-Location $AppPath

Write-Host "[*] Starting DEVELOPMENT environment..."

# Check if Docker is available by running the `docker` command
Write-Host '[*] Validating docker...'
$docker = Test-Docker
if ($docker -ne 'true') {
    throw '[!] Docker is not installed or active'
}
Enable-Docker

# Basic paths
Write-Host '[*] Creating APP directories...'
New-Item -ItemType Directory -Path $AppPath -Force
Copy-Item -Path "$SourcePath/develop.env" -Destination "$AppPath/.env" -Force
New-Item -ItemType Directory -Path "$AppPath/consul/etc" -Force

# Importing the .env file and exporting the variables
$environmentFile = "$SourcePath/develop.env"
Write-Host "[*] Environment variables are:" 
$env:HOSTNAMEID=$(hostname)
$env:HOSTNAME=$env:COMPUTERNAME
$env:DOCKER_MANAGERS=1
$env:DOCKER_INFRAS=1
Write-Host "[*] ....HOSTNAME: $env:HOSTNAME" 
Write-Host "[*] ....DOCKER_PUBLIC_IP: $env:DOCKER_PUBLIC_IP" 
Write-Host "[*] ....DOCKER_MANGER_COUNT: $env:DOCKER_MANAGER_COUNT" 
if (Test-Path $environmentFile) {
    Get-Content $environmentFile | ForEach-Object {
        $key, $value = $_ -split '='
        [System.Environment]::SetEnvironmentVariable($key.Trim(), $value.Trim(), [System.EnvironmentVariableTarget]::Process)
        Write-Host "[*] ....$key : $value" 
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
        Write-Host "[*] ....Set environment variable $key : ***"
    }
}
else {
    Write-Host "[!] ....Secrets file not found at $SecretsPath"
    return
}

# Loop through all source services and configure the application
$Selection = @()

Get-ChildItem -Path $SourcePath -Directory | ForEach-Object {

    $ServiceName = $_.Name
    $ServicePath = $_.FullName
    $ServiceUpper = $ServiceName.ToUpper()
    $ServiceFile = Join-Path -Path $ServicePath -ChildPath "service.json"
    $ServiceData = {}
    $AppServicePath = Join-Path -Path $AppPath -ChildPath $ServiceName

    Write-Host "[*] Configuring $ServiceUpper ..."

    if (Test-Path $ServiceFile) {
        try { 
            $ServiceData = Get-Content $ServiceFile | ConvertFrom-Json
        } catch {
            Write-Host "[!] ....Error reading $ServiceFile" -ForegroundColor Red
            return
        }
    } else {
        Write-Host "[!] ....Skipping $ServiceName : No service.json found." -ForegroundColor Yellow
        return
    }

    # Create the paths
    if ($ServiceData.service.paths) {
        foreach ($Entry in $ServiceData.service.paths) {
            $TargetPath = Join-Path -Path $AppServicePath -ChildPath $Entry.path

            if (-Not (Test-Path $TargetPath)) {
                Write-Host "[*] ....Creating directory: $TargetPath"
                New-Item -ItemType Directory -Path $TargetPath -Force | Out-Null
            }

            if($Entry.path -eq "conf") {
                Write-Host "[*] ....Copying configuration files to $TargetPath"
                Copy-Item -Path "$ServicePath/*" -Destination $targetPath -Recurse -Force
            }
            
            if($Entry.path -eq "logs") {
                Write-Host "[*] ....Clearing logs"
                Remove-Item -Path "$TargetPath/*" -Recurse -Force
            }
        }
    } else {
        Write-Host "[*] ....No servicepaths found in $ServiceData" -ForegroundColor DarkGray
    }

    # Convert Templates if needed
    Get-ChildItem -Path "$AppServicePath/conf" -Recurse -Filter "*.template.*" | ForEach-Object {
        Write-Host "[*] ....Merging template for $($_.FullName)"
        $TemplateFile = $_.FullName
        $OutputFile = $TemplateFile -replace '\.template\.', '.'
        if (Test-Path -Path $TemplateFile) {
            Merge-Template -InputFile $TemplateFile -OutputFile $OutputFile
        }
    }

    # Copy to the consul configuration directory
    Copy-Item -Path "$AppServicePath/conf/consul.json" -Destination "$AppPath/consul/etc/consul.$ServiceName.json" -Force
    
    # Execute extra development if needed
    $AppDevScript = "$AppServicePath/conf/develop.ps1"
    if (Test-Path $AppDevScript){
        . $AppDevScript
    }

    if ( 
        ($ServiceData.service.groups -contains $Group) -or 
        ($Services -contains $ServiceData.service.id) -or
        ($Group -eq "" -and $Services -eq @())
    ) {
        $expanded = Merge-EnvironmentVariables -InputString $ServiceData.service.endpoint
        $Selection += [PSCustomObject]@{
            id       = $ServiceData.service.id
            priority = $ServiceData.service.priority
            endpoint = $expanded
        }
        Write-Host "[*] ....Service $ServiceName SELECTED"
    }

    Write-Host "[+] Configuring $ServiceUpper ... DONE"
}

$SortedServices = $Selection | Sort-Object Priority
foreach ($service in $SortedServices) {
    $ServicePath = Join-Path -Path $AppPath -ChildPath $service.id 
    $ComposeFile = "$ServicePath/conf/compose.yml"

    #Write-Host "[*] Validating Docker Compose for " + $service.id.ToUpper()
    #docker compose -f $composeFile --env-file $environmentFile config
    Write-Host "[*] Deploying $ServiceName stack..."
    docker stack deploy -c $ComposeFile $Stack --detach=true
}

# DEBUG AND TEST

# Build a list of endpoint URLs based on $Selection
# Add the additional fixed browser arguments
$Urls = $SortedServices | ForEach-Object { $_.endpoint }
$BrowserArgs = @(
    "--inprivate",
    "--ignore-certificate-errors",
    "--ignore-urlfetcher-cert-requests",
    "--start-maximized",
    #"--start-minimized",
    "--new-window"
)

# Combine URLs and browser arguments
# Launch Microsoft Edge with the combined arguments
$Arguments = $Urls + $BrowserArgs | Where-Object { $_ -ne "" }
Start-Process -FilePath "msedge.exe" -ArgumentList $Arguments
Write-Host "[*] Starting DEVELOPMENT environment...DONE"