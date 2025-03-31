locals {
  install_docker_script = [
    "cd /",  # Change to the root directory
    "echo 'Initializing server'",
    "echo 'Reading package list'",
    "apt-get -y update",
    "echo 'Install Apache utils'",
    "if checkgid &>>/dev/null; then",
    "  echo 'Apache utils are already installed'",
    "else",
    "  apt-get -y install apt-transport-https ca-certificates curl gnupg-agent software-properties-common apache2-utils",
    "fi",
    "echo 'Install JSON parser'",
    "if jq --help; then",
    "  echo 'JSON parser already installed'",
    "else",
    "  apt-get -y install jq",  # Install jq
    "  echo 'JSON parser installed'",
    "fi",
    "echo 'Install docker and docker compose'",
    "apt-get install -y apt-transport-https ca-certificates curl software-properties-common",
    "curl -fsSL https://download.docker.com/linux/ubuntu/gpg | apt-key add -",
    "add-apt-repository 'deb [arch=amd64] https://download.docker.com/linux/ubuntu focal stable'",
    "apt-get update -y",
    "apt-get install -y docker-ce docker-ce-cli containerd.io",
    "usermod -aG docker ubuntu",
    "curl -L \"https://github.com/docker/compose/releases/download/1.29.2/docker-compose-$(uname -s)-$(uname -m)\" -o /usr/local/bin/docker-compose",
    "chmod +x /usr/local/bin/docker-compose",
    "docker --version",
    "docker-compose --version"
  ]
}
