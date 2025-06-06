# Configure and run KEYCLOAK
Write-Host 'Configuring KEYCLOAK ... for DOCKER'
Move-Item -Path "$AppPath/keycloak/conf/consul.keycloak.json" -Destination "$AppPath/consul/etc" -Force
Write-Host 'Configuring KEYCLOAK ... Done'