#!/bin/bash
set -e
echo "[*] Deploying REDISINSIGHT to remote server $(hostname)..."
create_service_paths "redisinsight"
echo "[*] Deploying REDISINSIGHT to remote server $(hostname)...DONE"
