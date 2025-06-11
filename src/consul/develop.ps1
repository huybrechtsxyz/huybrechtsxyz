# Configure and run service
Write-Host "[*] ....Configuring service ... "
Copy-Item -Path "$AppPathConf/consul/conf/config.json" -Destination "$AppPathConf/consul/etc/config.json" -Force
Write-Host "[+] ....Configuring service ... OK"