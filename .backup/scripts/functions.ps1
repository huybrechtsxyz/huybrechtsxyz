# VARIABLES
$ProjectGuid = "acbede3b-f8f8-41b2-ad78-d1f3e176949f"
$SecretsPath = "$env:APPDATA\Microsoft\UserSecrets\$ProjectGuid\secrets.json"

# See if docker is installed
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
    Enable-DockerSwarm

    #$wanNetworkInfo = 
    Enable-DockerSwarm -networkName "wan-platform"
    #$lanNetworkInfo = 
    Enable-DockerSwarm -networkName "lan-platform"
    #$devNetworkInfo = 
    Enable-DockerSwarm -networkName "lan-develop"

    $nodeName = docker node ls --format '{{.Hostname}}' | Where-Object { $_ -eq $hostname }
    docker node update --label-add postgres=true $nodeName
    docker node update --label-add role=manager $nodeName
    docker node update --label-add role=infra $nodeName
    docker node update --label-add role=worker $nodeName

    Set-DockerSecrets
    
    Write-Host "Docker configuration completed successfully."
}

# FUNCTION: Ensure Docker Swarm is initialized
function Enable-DockerSwarm {
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
function Enable-DockerNetwork([string]$networkName) {
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

# FUNCTION: Read jsons and create docker secrets
function Set-DockerSecrets {
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

function Confirm-Compose {
    param (
        [string]$ComposeFile,
        [string]$ServiceName
    )
    Write-Host "Validating Docker Compose for $ServiceName..."
    docker compose -f $ComposeFile --env-file $EnvironmentFile config | Out-Null
    if ($LASTEXITCODE -ne 0) {
        Write-Host "Invalid Docker Compose for $ServiceName" -ForegroundColor Red
        return $false
    }
    return $true
}
