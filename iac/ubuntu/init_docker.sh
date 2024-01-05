###################################
# Prerequisites

# Update the list of packages
sudo apt update

# Install pre-requisite packages.
sudo apt install apt-transport-https ca-certificates curl software-properties-common

# Then add the GPG key for the official Docker repository to your system:
curl -fsSL https://download.docker.com/linux/ubuntu/gpg | sudo gpg --dearmor -o /usr/share/keyrings/docker-archive-keyring.gpg

# Add the Docker repository to APT sources:
echo "deb [arch=$(dpkg --print-architecture) signed-by=/usr/share/keyrings/docker-archive-keyring.gpg] https://download.docker.com/linux/ubuntu $(lsb_release -cs) stable" | sudo tee /etc/apt/sources.list.d/docker.list > /dev/null

# Update your existing list of packages again for the addition to be recognized:
sudo apt update

# Make sure you are about to install from the Docker repo instead of the default Ubuntu repo
apt-cache policy docker-ce

# Finally, install Docker:
sudo apt install docker-ce

# Docker should now be installed, the daemon started, and the process enabled to start on boot. 
sudo systemctl status docker

# Executing the Docker Command Without Sudo 
sudo usermod -aG docker ${USER}

# To apply the new group membership, log out of the server and back in, or type the following
su - ${USER}

# Confirm that your user is now added to the docker group by typing:
groups

# Install docker compose
mkdir -p ~/.docker/cli-plugins/

curl curl -SL https://github.com/docker/compose/releases/download/v2.3.3/docker-compose-linux-x86_64 -o ~/.docker/cli-plugins/docker-compose

chmod +x ~/.docker/cli-plugins/docker-compose

docker compose version

# Enable docker compose
echo "Initialize docker swarm";
case "$(docker info --format '{{.Swarm.LocalNodeState}}')" in 
  inactive)
    docker swarm init;
    echo "Docker Swarm Node is initializing";;
  pending)
    echo "Docker Swarm Node is not in a swarm cluster";;
  active)
    echo "Docker Swarm Node is in a swarm cluster";;
  locked)
    echo "Docker Swarm Node is in a locked swarm cluster";;
  error)
    echo "Docker Swarm Node is in an error state";;
  *)
    echo "Docker Swarm Node is in an unknown state $(docker info --format '{{.Swarm.LocalNodeState}}')";;
esac