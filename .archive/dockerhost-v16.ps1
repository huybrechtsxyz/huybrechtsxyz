[CmdletBinding()]
param (
   [Parameter(Mandatory=$true, HelpMessage = 'Environment options?')]
   [int] $options = 0
   # 0 : Default
   # 9 : Full rebuild and docker pull
)

. ".\src\functions.ps1"

# SYSTEM
Write-Output 'Starting Docker Host environment...'
if($options -ne 0) {
   Write-Output 'Creating Docker Host environment...'
   if(Test-Path -Path c:\App) {
       Remove-Item c:\App\* -Recurse -Force
   }
} 
else {
   Write-Output 'Updating Docker Host environment...'
}

# DOCKER IMAGES
if($options -eq 9) {
   Write-Output 'Pulling container images...'
   docker-compose -f ./src/elastic/search/docker-compose.local.yml pull
   docker-compose -f ./src/elastic/logstash/docker-compose.local.yml pull
   docker-compose -f ./src/elastic/kibana/docker-compose.local.yml pull
   docker-compose -f ./src/elastic/metric/docker-compose.local.yml pull
   docker-compose -f ./src/elastic/filebeat/docker-compose.local.yml pull
   docker-compose -f ./src/elastic/health/docker-compose.local.yml pull
   docker-compose -f ./src/consul/docker-compose.local.yml pull
   docker-compose -f ./src/vault/docker-compose.local.yml pull
   docker-compose -f ./src/mkdocs/docker-compose.local.yml pull
   docker-compose -f ./src/traefik/docker-compose.local.yml pull
   docker-compose -f ./src/whoami/docker-compose.local.yml pull
}

# CONSUL
New-Item -ItemType Directory -Force -Path .\app\consul
New-Item -ItemType Directory -Force -Path .\app\consul\config
New-Item -ItemType Directory -Force -Path .\app\consul\data

# VAULT
New-Item -ItemType Directory -Force -Path .\app\vault
New-Item -ItemType Directory -Force -Path .\app\vault\file
New-Item -ItemType Directory -Force -Path .\app\vault\logs
Copy-Item .\src\vault\vault.services.consul.json -Destination .\app\consul\config -Force

# ELASTIC
New-Item -ItemType Directory -Force -Path .\app\elastic
Copy-Item .\src\elastic\search\elasticsearch.services.consul.json -Destination .\app\consul\config -Force
Copy-Item .\src\elastic\logstash\logstash.services.consul.json -Destination .\app\consul\config -Force
Copy-Item .\src\elastic\kibana\kibana.services.consul.json -Destination .\app\consul\config -Force

# TRAEFIK
New-Item -ItemType Directory -Force -Path .\app\traefik
Copy-Item .\src\traefik\traefik.services.consul.json -Destination .\app\consul\config -Force

# MKDOCS
if(Test-Path -Path .\app\docs) { Remove-Item .\app\docs\* -Recurse -Force }
New-Item -ItemType Directory -Force -Path .\app\docs\
Copy-Item .\docs\* -Destination .\app\docs\ -Recurse -Force
Copy-Item .\src\mkdocs\mkdocs.yml -Destination .\app\mkdocs.yml -Force
Copy-Item .\src\mkdocs\mkdocs.services.consul.json -Destination .\app\consul\config\mkdocs.services.consul.json -Force

# WHOAMI
Copy-Item .\src\whoami\whoami.services.consul.json -Destination .\app\consul\config -Force

# DEPLOY CONTAINERS
docker stack deploy -c ./src/consul/docker-compose.local.yml local
docker stack deploy -c ./src/vault/docker-compose.local.yml local
docker stack deploy -c ./src/elastic/search/docker-compose.local.yml local
docker stack deploy -c ./src/elastic/logstash/docker-compose.local.yml local
docker stack deploy -c ./src/elastic/kibana/docker-compose.local.yml local
docker stack deploy -c ./src/elastic/filebeat/docker-compose.local.yml local
docker stack deploy -c ./src/elastic/health/docker-compose.local.yml local
docker stack deploy -c ./src/elastic/metric/docker-compose.local.yml local
docker stack deploy -c ./src/mkdocs/docker-compose.local.yml local
docker stack deploy -c ./src/traefik/docker-compose.local.yml local
docker stack deploy -c ./src/whoami/docker-compose.local.yml local

# DEBUG / TEST
Start-Process -FilePath "chrome.exe" -ArgumentList `
  "http://traefik.localhost:5000 `
   http://consul.localhost:5000 `
   http://vault.localhost:5000 `
   http://whoami.localhost:5000 `
   http://docs.localhost:5000 `
   http://elastic.localhost:5000 `
   http://logstash.localhost:5000 `
   http://kibana.localhost:5000 `
   --incognito --start-maximized --new-window"
Pause 'Press any key to stop containers'
Write-Output 'Stopping Docker Host environment...'

# STOPPING SERVICES
docker stack rm local

# SYSTEM
Write-Output 'Docker Host environment stopped.'