# VARIABLES
$ProjectGuid = "acbede3b-f8f8-41b2-ad78-d1f3e176949f"
$SecretsPath = "$env:APPDATA\Microsoft\UserSecrets\$ProjectGuid\secrets.json"

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
