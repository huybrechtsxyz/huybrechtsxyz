# Configure and run SEAWEED
Write-Host 'Configuring SEAWEED ... for DOCKER'
Move-Item -Path "$AppPath/seaweed/conf/consul.seaweed.json" -Destination "$AppPath/consul/etc" -Force
Write-Host 'Configuring SEAWEED ... Done'