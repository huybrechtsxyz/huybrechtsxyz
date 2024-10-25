[CmdletBinding()]
param (
   [Parameter(Mandatory=$true, HelpMessage = 'Environment options?')]
   [int] $options = 0
)

. ".\src\functions.ps1"

# SYSTEM
Write-Output 'Starting Local Host environment...'

# CONSUL SERVER/AGENT
$env:CONSUL_ADDR="http://127.0.0.1:8500"
Start-Process -FilePath ".\run\consul\consul.exe" -ArgumentList "agent", "-dev" -WindowStyle Minimized

# VAULT SERVER/AGENT
$env:VAULT_ADDR="http://127.0.0.1:8200"
Start-Process -FilePath ".\run\vault\vault.exe" -ArgumentList "server", "-dev" -WindowStyle Minimized

# DEBUG AND TEST
Start-Process -FilePath "chrome.exe" -ArgumentList `
   "http://localhost:8500 ",`
   "http://localhost:8200 ",`
   "--incognito --start-maximized --new-window"
Pause 'Press any key to stop debugging'
Write-Output 'Stopping Local Host environment...'

# STOPPING SERVICES
Stop-Process -Name 'vault' -ErrorAction Ignore
Stop-Process -Name 'consul' -ErrorAction Ignore

# SYSTEM
Write-Output 'Local Host environment stopped.'