#!/bin/bash

CD /

mkdir app
cd app
mkdir bin
mkdir data
mkdir logscd

DIR="${BASH_SOURCE%/*}"
if [[ ! -d "$DIR" ]]; then DIR="$PWD"; fi
. "$DIR/init_firewall.sh"
. "$DIR/init_powershell.sh"
. "$DIR/init_docker.sh"