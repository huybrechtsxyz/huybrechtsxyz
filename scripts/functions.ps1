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
    docker node update --label-add postgres=true $POSTGRES_NODE
    docker node update --label-add role=manager
    docker node update --label-add role=worker
    docker node update --label-add role=infra
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
