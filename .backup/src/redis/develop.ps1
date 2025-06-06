# Configure and run REDIS
Write-Host 'Configuring REDIS ... for DOCKER'
Move-Item -Path "$AppPath/redis/conf/consul.redis.json" -Destination "$AppPath/consul/etc" -Force
Write-Host 'Configuring REDIS ... Done'