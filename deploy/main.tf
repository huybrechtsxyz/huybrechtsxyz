terraform {
  required_providers {
    kamatera = {
      source = "Kamatera/kamatera"
    }
    random = {
      source  = "hashicorp/random"
      version = "~> 3.5"
    }
  }
  
  cloud {
    organization = "huybrechts-xyz"
    workspaces {
      name = "huybrechts-xyz-"
    } 
  } 
}

provider "kamatera" {
  api_client_id = var.api_key
  api_secret = var.api_secret
}

# define the data center we will create the server and all related resources in
# see the section below "Listing available data centers" for more details
data "kamatera_datacenter" "frankfurt" {
  country = "Germany"
  name = "Frankfurt"
}

# define the server image we will create the server with
# see the section below "Listing available public images" for more details
# also see "Using a private image" if you want to use a private image you created yourself
data "kamatera_image" "ubuntu" {
  datacenter_id = data.kamatera_datacenter.frankfurt.id
  os = "Ubuntu"
  code = "24.04 64bit"
}

# Create a random suffix resource
resource "random_string" "suffix" {
  length  = 4
  upper   = false
  special = false
}

# Set up private network
resource "kamatera_network" "private-lan" {
  datacenter_id = data.kamatera_datacenter.frankfurt.id
  name = "vlan-${var.environment}-${random_string.suffix.result}"

  subnet {
    ip = "10.0.0.0"
    bit = 23
  }
}

# Provision manager 
resource "kamatera_server" "manager" {
  count             = var.manager_count
  name              = "srv-${var.environment}-manager-${count.index + 1}-${random_string.suffix.result}"
  image_id          = data.kamatera_image.ubuntu.id
  datacenter_id     = data.kamatera_datacenter.frankfurt.id
  cpu_cores         = var.manager_cpu
  cpu_type          = "A"
  ram_mb            = var.manager_ram
  disk_sizes_gb     = [ var.manager_disk_size ]
  billing_cycle     = "hourly"
  power_on          = true

  network {
    name = "wan"
  }

  network {
    name = kamatera_network.private-lan.full_name
  }

  # Use remote-exec to run a script on the remote instance
  provisioner "remote-exec" {
    inline = [
      "apt update -y",
      "apt install -y docker.io",
      "systemctl enable docker",
      "systemctl start docker",

      # Get private IP and initialize Swarm
      "PRIVATE_IP=$(hostname -I | awk '{print $2}')",
      "docker swarm init --advertise-addr $PRIVATE_IP",
      
      # Generate the worker join token
      "docker swarm join-token worker -q > /root/swarm-token"
    ]

    connection {
      type     = "ssh"
      user     = "root"
      password = var.password
      host     = self.public_ips[0] # Make sure you're using the correct IP
    }
  }
}

# Provision workernode 
resource "kamatera_server" "worker" {
  count             = var.worker_count
  name              = "srv-${var.environment}-worker-${count.index + 1}-${random_string.suffix.result}"
  image_id          = data.kamatera_image.ubuntu.id
  datacenter_id     = data.kamatera_datacenter.frankfurt.id
  cpu_cores         = var.worker_cpu
  cpu_type          = "A"
  ram_mb            = var.worker_ram
  disk_sizes_gb     = [ var.worker_disk_size, var.block_storage_size ]
  billing_cycle     = "hourly"
  power_on          = true

  network {
    name = "wan"
  }

  network {
    name = kamatera_network.private-lan.full_name
  }

  # Use remote-exec to run the join command on the worker node
  provisioner "remote-exec" {
    inline = [
      "apt update -y",
      "apt install -y docker.io",
      "systemctl enable docker",
      "systemctl start docker",

      # Join the swarm
      "MANAGER_PRIVATE_IP=<manager_private_ip>",
      "JOIN_TOKEN=$(cat /root/swarm-token)",
      "docker swarm join --token $JOIN_TOKEN $MANAGER_PRIVATE_IP:2377"
    ]

    connection {
      type     = "ssh"
      user     = "root"
      password = var.password
      host     = self.public_ips[0] # Use worker node's IP
    }
  }
}