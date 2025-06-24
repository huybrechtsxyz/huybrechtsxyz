# Configure and run service
Write-Host "[*] ....Configuring service ... "
Copy-Item -Path "$AppPath/consul/conf/config.json" -Destination "$AppPath/consul/etc/config.json" -Force
Write-Host "[+] ....Configuring service ... OK"