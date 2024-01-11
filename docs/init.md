## Install Docker

sudo -s
apt update
apt-get upgrade -y
apt install mailutils apt-transport-https ca-certificates curl software-properties-common apache2-utils

curl -fsSL https://download.docker.com/linux/ubuntu/gpg | sudo apt-key add -
add-apt-repository "deb [arch=amd64] https://download.docker.com/linux/ubuntu focal stable"
apt update
apt install docker-ce -y

# Optionally limit log size
journalctl --vacuum-size=100M    

systemctl start docker

## Install docker-compose

sudo -s
curl -L https://github.com/docker/compose/releases/download/1.26.0/docker-compose-`uname -s`-`uname -m` -o /usr/local/bin/docker-compose
chmod +x /usr/local/bin/docker-compose

## Create a deployment user

adduser -g docker github
su - github
ssh-keygen

mv .ssh/id_rsa.pub .ssh/authorized_keys
cat .ssh/id_rsa
rm .ssh/id_rsa
cat .ssh/authorized_keys

exit



