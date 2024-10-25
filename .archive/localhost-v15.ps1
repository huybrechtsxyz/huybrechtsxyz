[CmdletBinding()]
param (
    [Parameter(Mandatory=$true, HelpMessage = 'Recreate environment? 0: No, 1: Yes')]
    [int] $recreateEnvironment = 0
)

Function Pause ($message)
{
    # Check if running Powershell ISE
    if ($psISE)
    {
        Add-Type -AssemblyName System.Windows.Forms
        [System.Windows.Forms.MessageBox]::Show("$message")
    }
    else
    {
        Write-Host "$message" -ForegroundColor Yellow
        $host.ui.RawUI.ReadKey("NoEcho,IncludeKeyDown")
    }
}

# SYSTEM
Write-Output 'Building localhost environment...'
if($recreateEnvironment -eq 1) {
    Write-Output 'Creating localhost environment...'
    if(Test-Path -Path c:\App) {
        Remove-Item c:\App\* -Recurse -Force
    }
} 

# APP DIR
New-Item -ItemType Directory -Force -Path C:\App\

# CONSUL SERVER
Copy-Item .\src\consul\docker-compose.localhost.yml -Destination c:\App\docker-compose.consul.yml -Force
New-Item -ItemType Directory -Force -Path C:\App\consul\config
New-Item -ItemType Directory -Force -Path C:\App\consul\data
Copy-Item .\src\consul\consul-server.localhost.json -Destination c:\App\consul\config\consul-server.json -Force

# TRAEFIK
New-Item -ItemType Directory -Force -Path C:\App\traefik
Copy-Item .\src\traefik\docker-compose.localhost.yml -Destination c:\App\docker-compose.traefik.yml -Force

# WHOAMI
Copy-Item .\src\whoami\docker-compose.localhost.yml -Destination c:\App\docker-compose.whoami.yml -Force
Copy-Item .\src\whoami\consul-service.localhost.json -Destination c:\App\consul\config\consul-service.whoami.json -Force

# MKDOCS
Copy-Item .\src\mkdocs\docker-compose.localhost.yml -Destination c:\App\docker-compose.mkdocs.yml -Force
Copy-Item .\src\mkdocs\consul-service.localhost.json -Destination c:\App\consul\config\consul-service.mkdocs.json -Force
Copy-Item .\src\mkdocs\mkdocs.yml -Destination c:\App\mkdocs.yml -Force
if(Test-Path -Path c:\App\docs) { Remove-Item c:\App\docs\* -Recurse -Force }
New-Item -ItemType Directory -Force -Path C:\App\docs\
Copy-Item .\docs\* -Destination c:\App\docs\ -Recurse -Force

# DOCKER IMAGES
if($recreateEnvironment -eq 1) {
    Write-Output 'Pulling container images...'
    docker-compose -f c:\App\docker-compose.consul.yml pull
    docker-compose -f c:\App\docker-compose.traefik.yml pull
    docker-compose -f c:\App\docker-compose.whoami.yml pull
    docker-compose -f c:\App\docker-compose.mkdocs.yml pull
}

# DEPLOY DOCKER
Write-Output 'Starting localhost environment...'
docker stack deploy -c c:\App\docker-compose.whoami.yml local
docker stack deploy -c c:\App\docker-compose.consul.yml local
docker stack deploy -c c:\App\docker-compose.traefik.yml local
docker stack deploy -c c:\App\docker-compose.mkdocs.yml local

# CHROME
Start-Process "chrome.exe" -ArgumentList "`
    http://traefik.localhost:5000`
    http://consul.localhost:5000`
    http://whoami.localhost:5000`
    http://mkdocs.localhost:5000`
    --incognito --start-maximized --new-window"
Pause('Press any key to continue...')

# CLEAN AND EXIT
docker stack rm local
