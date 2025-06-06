# Configure and run service
Write-Host "[*] ....Configuring service ... "
Move-Item -Path "$AppPath/consul/conf/consul-config.json" -Destination "$AppPath/consul/etc/consul-config.json" -Force
Write-Host "[+] ....Configuring service ... OK"