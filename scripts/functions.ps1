# VARIABLES
$SourcePath = "$RootPath/src"
$AppPath = "$RootPath/.app"
$ProjectGuid = "acbede3b-f8f8-41b2-ad78-d1f3e176949f"
$SecretsPath = "$env:APPDATA\Microsoft\UserSecrets\$ProjectGuid\secrets.json"

# FUNCTION: Check if Docker is available by running the `docker` command
function Test-Docker {
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

# FUNCTION: Configure required docker networks
function Enable-Docker {
    Write-Host "Docker configuration in progress."
    Confirm-SwarmInitialized
    #$wanNetworkInfo = Confirm-NetworkExistsAndValid -networkName "wan-$env:WORKSPACE"
    #$lanNetworkInfo = Confirm-NetworkExistsAndValid -networkName "lan-$env:WORKSPACE"
    Confirm-NetworkExistsAndValid -networkName "wan-$env:WORKSPACE"
    Confirm-NetworkExistsAndValid -networkName "lan-$env:WORKSPACE"
    Confirm-NetworkExistsAndValid -networkName "lan-develop"
    Set-DockerSecrets
    $NODE=docker node ls --format '{{.Hostname}}'
    docker node update --label-add manager=true $NODE
    docker node update --label-add worker=true $NODE
    docker node update --label-add infra=true $NODE
    docker node update --label-add postgres=true $NODE
    docker node update --label-add minio=true $NODE
    Write-Host "Docker configuration completed successfully."
}

# FUNCTION: Ensure Docker Swarm is initialized
function Confirm-SwarmInitialized {
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
function Confirm-NetworkExistsAndValid([string]$networkName) {
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

function Set-DockerSecrets {
    if (-not (Test-Path -Path $SecretsPath)){
        throw 'User secrets not found'
    }

    $Secrets = Get-Content -Path $SecretsPath | ConvertFrom-Json
    foreach ($Key in $Secrets.PSObject.Properties.Name) {
        $Value = $Secrets.$Key
        Write-Host "Creating or updating secret: $Key"
        
        # Check if the secret exists before trying to remove it
        if (docker secret ls --format '{{.Name}}' | Where-Object { $_ -eq $Key }) {
            docker secret rm $Key | Out-Null
            Write-Host "Removed secret: $Key"
        } else {
            Write-Host "Secret $Key does not exist. Skipping removal."
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

# FUNCTION: Apply variable template
function Merge-Template {
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
