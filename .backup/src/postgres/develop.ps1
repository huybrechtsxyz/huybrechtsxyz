# Configure and run POSTGRES
Write-Host 'Configuring POSTGRES ... for DOCKER'
Move-Item -Path "$AppPath/postgres/conf/consul.postgres.json" -Destination "$AppPath/consul/etc" -Force
Write-Host 'Configuring POSTGRES ... Done'