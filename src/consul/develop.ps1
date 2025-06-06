# Configure and run service
$ServiceName="consul"
$ServiceUpper=$ServiceName.ToUpper()
Write-Host "Configuring $ServiceUpper ... for DOCKER"
Move-Item -Path "$AppPath/consul/conf/consul-config.json" -Destination "$AppPath/consul/etc/consul-config.json" -Force
Write-Host "Configuring $ServiceUpper ... Done"