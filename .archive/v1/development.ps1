
# Function Configure and run BACKUP
function Invoke-Backup {
    Write-Host 'Configuring BACKUP ...'
    Copy-Item -Path "./src/backup/*" -Destination $baseDir -Recurse
    if ($docker -eq 'true') {
        Write-Host 'Configuring BACKUP ... for DOCKER'
    } else {
        Write-Host 'Configuring BACKUP ... skipping'
    }
    Write-Host 'Configuring BACKUP ... done'
}

# Function Configure and run Consul
function Invoke-Consul {
    Write-Host 'Configuring CONSUL ...'
    $consulDir = "$baseDir/consul"
    $consulConf = "$consulDir/conf"
    $consulData = "$consulDir/data"
    New-Item -ItemType Directory -Path $consulDir, $consulConf, $consulData -Force
    $consulDir = Resolve-Path -Path $consulDir
    $consulExe = "$consulDir/consul.exe"
    Copy-Item -Path "./src/consul/*develop*" -Destination $consulConf -Recurse
    
    if ($docker -eq 'true') {
        Write-Host 'Configuring CONSUL ... for DOCKER'

    } else {
        Write-Host 'Configuring CONSUL ... for LOCALHOST'
        if (-Not (Test-Path -Path $consulExe)) {
            Write-Output " -- Extracting Consul..."
            Expand-ZipFile -zipFile "./zips/consul_1.20.1.zip" -extractPath $consulDir
        }
        Write-Host " -- Setting environment ..."
        $env:CONSUL_ADDR="http://127.0.0.1:8500"
        Write-Host " -- Starting executable ..."
        Start-Process -FilePath $consulExe -ArgumentList "agent", "-dev", "-config-dir $consulData" -WindowStyle Minimized
    }
    Write-Host 'Configuring CONSUL ... done'
}

# Function Configure and run Loki
function Invoke-Loki {
    Write-Host 'Configuring LOKI ...'
    $lokiDir = "$baseDir/loki"
    $lokiConf = "$lokiDir/conf"
    $lokiData = "$lokiDir/data"
    New-Item -ItemType Directory -Path $lokiDir, $lokiConf, $lokiData -Force
    Copy-Item -Path "./src/loki/*" -Destination $lokiConf -Recurse
    if ($docker -eq 'true') {
        Write-Host 'Configuring LOKI ... for DOCKER'
    } else {
        Write-Host 'Configuring LOKI ... skipping'
    }
    Write-Host 'Configuring LOKI ... done'
}

# Function Configure and run minio
function Invoke-Minio {
    Write-Host 'Configuring MINIO ...'
    $minioDir = "$baseDir/minio"
    $minioData = "$minioDir/data"
    New-Item -ItemType Directory -Path $minioDir, $minioData -Force
    $minioDir = Resolve-Path -Path $minioDir
    $minioExe = "$minioDir/minio.exe"
    Write-Host " -- Setting environment ..."
    $env:MINIO_ROOT_USER="admin"
    $env:MINIO_ROOT_PASSWORD="password"

    if ($docker -eq 'true') {
        Write-Host 'Configuring MINIO ... for DOCKER'
    } else {
        Write-Host 'Configuring MINIO ... for LOCALHOST'
        if (-Not (Test-Path -Path $minioExe)) {
            Write-Output " -- Extracting minio..."
            Expand-ZipFile -zipFile "./zips/minio-v202411.zip" -extractPath $minioDir
        }
        Write-Host " -- Starting executable ..."
        Start-Process -FilePath $minioExe -ArgumentList 'server /data --console-address ":9001"' -WindowStyle Minimized
    }
    Write-Host 'Configuring MINIO ... done'
}



# Function Configure and run Prometheus
function Invoke-Prometheus {
    Write-Host 'Configuring PROMETHEUS ...'
    $prometheusDir = "$baseDir/prometheus"
    $prometheusConf = "$prometheusDir/conf"
    $prometheusData = "$prometheusDir/data"
    New-Item -ItemType Directory -Path $prometheusDir, $prometheusConf, $prometheusData -Force
    Copy-Item -Path "./src/prometheus/*" -Destination $prometheusConf -Recurse
    if ($docker -eq 'true') {
        Write-Host 'Configuring PROMETHEUS ... for DOCKER'
    } else {
        Write-Host 'Configuring PROMETHEUS ... skipping'
    }
    Write-Host 'Configuring PROMETHEUS ... done'
}

# Function Configure and run Thanos
function Invoke-Thanos {
    Write-Host 'Configuring THANOS ...'
    $thanosDir = "$baseDir/thanos"
    $thanosConf = "$thanosDir/conf"
    $thanosData = "$thanosDir/data"
    New-Item -ItemType Directory -Path $thanosDir, $thanosConf, $thanosData -Force
    Copy-Item -Path "./src/thanos/*" -Destination $thanosConf -Recurse
    if ($docker -eq 'true') {
        Write-Host 'Configuring THANOS ... for DOCKER'
    } else {
        Write-Host 'Configuring THANOS ... skipping'
    }
    Write-Host 'Configuring THANOS ... done'
}

# Function Configure and run Traefik
function Invoke-Traefik {
    Write-Host 'Configuring TRAEFIK ...'
    $traefikDir = "$baseDir/traefik"
    $traefikConf = "$traefikDir/conf"
    $traefikData = "$traefikDir/data"
    $traefikLogs = "$traefikDir/logs"
    New-Item -ItemType Directory -Path $traefikDir, $traefikData, $traefikConf, $traefikLogs -Force
    $traefikConf = Resolve-Path -Path $traefikConf
    Copy-Item -Path "./src/traefik/*develop*" -Destination $traefikConf -Recurse
    if ($docker -eq 'true') {
        Write-Host 'Configuring TRAEFIK ... for DOCKER'
    } else {
        Write-Host 'Configuring TRAEFIK ... skipping'
    }
    Write-Host 'Configuring TRAEFIK ... done'
}
