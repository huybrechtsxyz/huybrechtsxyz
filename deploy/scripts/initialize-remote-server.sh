#!/bin/bash
set -euo pipefail
cd /

configure_swarm() {
  local hostname
  hostname=$(hostname)
  echo "[*] Configuring Docker Swarm on $hostname..."

  if [ "$(docker info --format '{{.Swarm.LocalNodeState}}' 2>/dev/null)" = "active" ]; then
    echo "[*] Node already part of a Swarm. Skipping initialization/joining."
    if [[ "$hostname" == *"manager-1"* ]]; then
      docker swarm join-token manager -q > /tmp/manager_token.txt
      docker swarm join-token worker -q > /tmp/worker_token.txt
    fi
    return
  fi

  if [[ "$hostname" == *"manager-1"* ]]; then
    echo "[*] Initializing new Swarm cluster..."
    docker swarm init --advertise-addr "$PRIVATE_IP"
    mkdir -p /tmp
    chmod 1777 /tmp
    docker swarm join-token manager -q > /tmp/manager_token.txt
    docker swarm join-token worker -q > /tmp/worker_token.txt
    echo "[*] Saved manager and worker join tokens."
  else
    echo "[*] Joining existing Swarm cluster on $MANAGER_IP..."
    SSH_OPTS="-o StrictHostKeyChecking=no -o ConnectTimeout=10"
    for i in {1..12}; do
      if ssh $SSH_OPTS root@$MANAGER_IP 'test -f /tmp/manager_token.txt && test -f /tmp/worker_token.txt'; then
        echo "[*] Swarm tokens are available on $MANAGER_IP"
        break
      fi
      echo "[!] Attempt $i: Waiting for Swarm tokens..."
      sleep 5
    done

    if ! ssh $SSH_OPTS root@$MANAGER_IP 'test -f /tmp/manager_token.txt && test -f /tmp/worker_token.txt'; then
      echo "[x] Timed out waiting for Swarm tokens. Exiting."
      exit 1
    fi

    MANAGER_JOIN_TOKEN=$(ssh $SSH_OPTS root@$MANAGER_IP 'cat /tmp/manager_token.txt')
    WORKER_JOIN_TOKEN=$(ssh $SSH_OPTS root@$MANAGER_IP 'cat /tmp/worker_token.txt')

    if [[ "$hostname" == *"manager-"* ]]; then
      echo "[*] Joining as Swarm Manager..."
      docker swarm join --token "$MANAGER_JOIN_TOKEN" $MANAGER_IP:2377 --advertise-addr "$PRIVATE_IP"
    else
      echo "[*] Joining as Swarm Worker..."
      docker swarm join --token "$WORKER_JOIN_TOKEN" $MANAGER_IP:2377 --advertise-addr "$PRIVATE_IP"
    fi

    echo "[*] Successfully joined Swarm cluster"
  fi
}

main() {
  export PRIVATE_IP="$PRIVATE_IP"
  export MANAGER_IP="$MANAGER_IP"
  configure_swarm
}
main
