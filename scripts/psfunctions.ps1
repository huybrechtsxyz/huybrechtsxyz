# VARIABLES
$ProjectGuid = "acbede3b-f8f8-41b2-ad78-d1f3e176949f"
$SecretsPath = "$env:APPDATA\Microsoft\UserSecrets\$ProjectGuid\secrets.json"

# FUNCTION: Apply variable template
function Apply-Template {
    param(
        [string]$templateFile,
        [string]$configFile
    )
    # Read the content of the template file
    $templateContent = Get-Content -Raw -Path "$templateFile"

    # Get all environment variables that are in the form of $<VAR_NAME>
    $envVars = [System.Environment]::GetEnvironmentVariables()

    # Loop through each environment variable and apply the replacement
    foreach ($key in $envVars.Keys) {
        # Construct the pattern to match the $<VAR_NAME> in the template
        #$pattern = "\$($key)"
        # Replace the placeholder with the actual value of the environment variable
        #$templateContent = $templateContent -Replace $pattern, $envVars[$key]

        $templateContent = $templateContent.Replace("`$($key)", "`$($envVars[$key])")
    }

    # Save the modified content to the config file
    Set-Content -Path "$configFile" -Value $templateContent

    # Remove the template file
    Remove-Item -Path "$templateFile" -Force
}

# FUNCTION: Configure required docker networks
function Configure-Docker {
    # Check if the current node is already part of a swarm
    $swarmState = docker info --format '{{.Swarm.LocalNodeState}}'

    if ($swarmState -eq "active") {
        Write-Host "Swarm is already initialized." -ForegroundColor Green
    } else {
        # Initialize a new swarm
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

    # Check and create the "public" network if it doesn't exist
    $publicNetwork = "wan"
    if (!(docker network ls --format '{{.Name}}' | Select-String -Pattern "^$publicNetwork$")) {
        Write-Host "Creating external network '$publicNetwork'..."
        docker network create --subnet=10.0.0.0/24 --driver overlay $publicNetwork
    } else {
        Write-Host "Network '$publicNetwork' already exists."
    }

    # Get the public ip address and subnet
    # $publicDetails = docker network inspect $publicNetwork | ConvertFrom-Json
    # $publicSubnetIP = $publicDetails.IPAM.Config[0].Subnet # For Subnet
    # $publicGatewayIP = $publicDetails.IPAM.Config[0].Gateway # For Gateway
    # Write-Host "Public Network IP: $publicGatewayIP"
    # [System.Environment]::SetEnvironmentVariable("DOCKER_PUBLIC_IP", $publicGatewayIP, [System.EnvironmentVariableTarget]::Process)

    # Check and create the "private" network if it doesn't exist
    $privateNetwork = "lan"
    if (!(docker network ls --format '{{.Name}}' | Select-String -Pattern "^$privateNetwork$")) {
        Write-Host "Creating external network '$privateNetwork'..."
        docker network create --subnet=10.0.0.0/24 --driver overlay $privateNetwork
    } else {
        Write-Host "Network '$privateNetwork' already exists."
    }

    # Get the private ip address and subnet
    # $privateDetails = docker network inspect $privateNetwork | ConvertFrom-Json
    #$privateSubnetIP = $privateDetails.IPAM.Config[0].Subnet # For Subnet
    # $privateGatewayIP = $privateDetails.IPAM.Config[0].Gateway # For Gateway
    # Write-Host "Private Network IP: $privateGatewayIP"
    # [System.Environment]::SetEnvironmentVariable("DOCKER_PRIVATE_IP", $privateGatewayIP, [System.EnvironmentVariableTarget]::Process)

    # Get the number of manager nodes
    # $managerNodes = docker node ls --filter "role=manager" --quiet
    # $managerCount = ($managerNodes | Measure-Object -Line).Lines
    # Write-Host "Number of manager nodes in the Docker Swarm: $managerCount"
    # [System.Environment]::SetEnvironmentVariable("DOCKER_MANAGER_COUNT", $managerCount)
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
