# VARIABLES
$ProjectGuid = "acbede3b-f8f8-41b2-ad78-d1f3e176949f"
$SecretsPath = "$env:APPDATA\Microsoft\UserSecrets\$ProjectGuid\secrets.json"

# FUNCTION: Apply variable template
function Apply-Template {
    param (
        [Parameter(Mandatory=$true)][string]$InputFile,
        [Parameter(Mandatory=$true)][string]$OutputFile
    )

    if (-not (Test-Path $InputFile)) {
        throw "Input file '$InputFile' not found."
    }

    $content = Get-Content -Path $InputFile -Raw

    $processed = [regex]::Replace($content, '\$(\w+)|\$\{(\w+)\}', {
        param($match)
        $varName = if ($match.Groups[1].Success) { $match.Groups[1].Value } else { $match.Groups[2].Value }
        $value = [System.Environment]::GetEnvironmentVariable($varName)
        return $value
    })

    Set-Content -Path $OutputFile -Value $processed
}

# FUNCTION: Ensure Docker Swarm is initialized
function Ensure-SwarmInitialized {
    $swarmState = docker info --format '{{.Swarm.LocalNodeState}}'
    if ($swarmState -eq "active") {
        Write-Host "Swarm is already initialized." -ForegroundColor Green
    } else {
        Write-Host "Initializing a new Docker Swarm..." -ForegroundColor Yellow
        $initResult = docker swarm init
        if ($initResult -like "*is now a manager*") {
            Write-Host "Swarm initialized successfully!" -ForegroundColor Green
        } else {
            Write-Host "Swarm initialization failed. Check Docker logs for more information." -ForegroundColor Red
            Write-Host $initResult
            exit 1
        }
    }
}

# FUNCTION: Ensure a docker network exists and is valid
function Ensure-NetworkExistsAndValid([string]$networkName) {
    if (-not (docker network ls --format '{{.Name}}' | Select-String -Pattern "^$networkName$")) {
        Write-Host "Creating external network '$networkName'..."
        docker network create --driver overlay --attachable $networkName
    } else {
        Write-Host "Network '$networkName' already exists."
    }

    $networkInfo = docker network inspect $networkName | ConvertFrom-Json
    $driver = $networkInfo[0].Driver
    $scope = $networkInfo[0].Scope

    Write-Host "Network '$networkName':"
    Write-Host "  Driver: $driver"
    Write-Host "  Scope:  $scope"

    if ($driver -ne "overlay" -or $scope -ne "swarm") {
        Write-Warning "Network '$networkName' is not a valid Swarm overlay network."
    } else {
        Write-Host "Network '$networkName' is a valid Swarm overlay network."
    }

    return $networkInfo
}

# FUNCTION: Configure required docker networks
function Configure-Docker {
    Write-Host "Docker configuration in progress."
    Ensure-SwarmInitialized
    $wanNetworkInfo = Ensure-NetworkExistsAndValid -networkName "wan"
    $lanNetworkInfo = Ensure-NetworkExistsAndValid -networkName "lan"
    docker node update --label-add postgres=true $POSTGRES_NODE
    Write-Host "Docker configuration completed successfully."
}

# FUNCTION: Add a label to a Docker node
function Add-DockerNodeLabel {
    param (
        [Parameter(Mandatory=$true)]
        [string]$NodeName,

        [Parameter(Mandatory=$true)]
        [string]$LabelKey
    )

    # Get the matching node's hostname
    $node = docker node ls --format '{{.Hostname}}' | Where-Object { $_ -eq $NodeName }

    if ($node) {
        # Add the label (e.g., postgres=true)
        docker node update --label-add "$LabelKey=true" $node
        Write-Host "Label '$LabelKey=true' added to node '$node'."
    } else {
        Write-Warning "Node '$NodeName' not found."
    }
}

# FUNCTION: Extract zipfile for local development
function Expand-ZipFile {
    param (
        [string]$zipFile,
        [string]$extractPath
    )

    # Check if the zip file exists
    if (Test-Path -Path $zipFile) {
        # Load the compression assembly and extract the zip file
        Add-Type -AssemblyName System.IO.Compression.FileSystem
        [System.IO.Compression.ZipFile]::ExtractToDirectory($zipFile, $extractPath)
        Write-Host "Zip file extracted successfully to $extractPath"
    } else {
        Write-Error "Zip file not found at $zipFile"
    }
}

# FUNCTION: Check if Docker is available by running the `docker` command
function Get-Docker {
    try {
        # Try running 'docker --version' to check if Docker is installed and available
        $dockerVersion = & docker --version 2>&1
        # If Docker is available, $dockerVersion should contain version information, otherwise it throws an error
        if ($dockerVersion -match "Docker version") {
            Write-Host " -- Docker is available"
            # Check if Docker is running
            try {
                $output = docker run --rm hello-world 2>&1
                if ($output -like "*docker: error during connect*") {
                    Write-Host " -- Docker is not running"
                    return $false
                } else {
                    Write-Host " -- Docker is running"
                    return $true
                }
            } catch {
                Write-Host " -- Docker is not running."
                return $false
            }
        } else {
            Write-Host " -- Docker not found"
            return $false
        }
    } catch {
        # Handle the case where Docker isn't available
        Write-Host " -- Docker is not installed."
        return $false
    }
}

# FUNCTION: Read secret.json
function Get-Secrets {
    if (-NOT(Test-Path $SecretsPath)) {
        Write-Host "Secrets file not found at $SecretsPath"
    }

    $secrets = Get-Content $SecretsPath | ConvertFrom-Json
    return $secrets
}

# FUNCTION: Set environment variables from secrets
function Get-SecretsAsVars {
    $secrets = Get-Secrets
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

# FUNCTION: Get environment variables from a file
function Get-EnvVarsFromFile {
    param (
        [string]$environmentFile
    )

    if (Test-Path $environmentFile) {
        Get-Content $environmentFile | ForEach-Object {
            $key, $value = $_ -split '='
            [System.Environment]::SetEnvironmentVariable($key.Trim(), $value.Trim(), [System.EnvironmentVariableTarget]::Process)
            Write-Host "Set environment variable $key : $value"
        }
    } else {
        Write-Host "Environment file not found at $environmentFile"
    }
}

# FUNCTION: Read jsons and create docker secrets
function Update-Secrets {
    if (-not (Test-Path -Path $SecretsPath)){
        throw 'User secrets not found'
    }

    $Secrets = Get-Content -Path $SecretsPath | ConvertFrom-Json
    foreach ($Key in $Secrets.PSObject.Properties.Name) {
        $Value = $Secrets.$Key
        Write-Host "Creating or updating secret: $Key"
        
        # Remove the secret if it already exists
        if (docker secret inspect $Key 2>&1) {
            docker secret rm $Key 2>&1
        }

        # Create the Docker secret
        $TempSecretFile = [System.IO.Path]::GetTempFileName()
        Set-Content -Path $TempSecretFile -Value $Value -NoNewLine
        $WrittenValue = Get-Content -Path $TempSecretFile -Raw
        if ($WrittenValue -ne $Value) {
            throw "The written secret value for '$Key' does not match the original."
        }
        docker secret create $Key $TempSecretFile
        Remove-Item -Path $TempSecretFile
    }
}
