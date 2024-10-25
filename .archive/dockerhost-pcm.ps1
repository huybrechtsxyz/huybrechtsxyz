[CmdletBinding()]
param (
  [Parameter(Mandatory = $true, HelpMessage = 'Environment options?')]
  [int] $options = 0
  # 0 : Nothing
  # 1 : Default
  # 8 : No rebuild and docker pull
  # 9 : Full rebuild and docker pull
)

. ".\src\functions.ps1"

# Check if docker is running
$docker = Get-Process 'com.docker.proxy' -ErrorAction SilentlyContinue
if ($docker.Id -eq 0) {
  Start-Process -FilePath "C:\Program Files\Docker\Docker\Docker Desktop.exe"
}

# SYSTEM
Write-Output 'Starting Docker Host environment...'
if ($options -gt 1) {
  Write-Output 'Creating Docker Host environment...'
  if (Test-Path -Path c:\app) {
    Remove-Item c:\app\* -Recurse -Force
  }
} 
else {
  Write-Output 'Updating Docker Host environment...'
}

# BUILD DOCKER IMAGE
if ($options -eq 9) {
  docker build -t pcmwebmvc:local -f "./src/PCM.WebMVC/dockerfile"
}

# DOCKER IMAGES
if ($options -eq 8 -or $options -eq 9) {
  Write-Output 'Pulling container images...'
  #docker-compose -f ./src/PCM.WebMVC/docker-compose.local.yml pull
  #docker-compose -f ./config/traefik/docker-compose.local.yml pull
  #docker-compose -f ./config/whoami/docker-compose.local.yml pull
}

# PCM WEBMC
New-Item -ItemType Directory -Force -Path .\app\data

# TRAEFIK
New-Item -ItemType Directory -Force -Path .\app\traefik

# DEPLOY CONTAINERS
#docker stack deploy -c ./src/PCM.WebMVC/docker-compose.local.yml local
#docker stack deploy -c ./config/traefik/docker-compose.local.yml local
#docker stack deploy -c ./config/whoami/docker-compose.local.yml local

# DEBUG / TEST
#Start-Process -FilePath "chrome.exe" -ArgumentList `
#  "http://localhost:8000 `
#  http://admin.localhost:8000/dashboard/#/ `
#  http://admin.localhost:8000/whoami `
#  --incognito --start-maximized --new-window"
#Pause 'Press any key to stop containers'
Write-Output 'Stopping Docker Host environment...'

# STOPPING SERVICES
#docker stack rm local

# SYSTEM
Write-Output 'Docker Host environment stopped.'