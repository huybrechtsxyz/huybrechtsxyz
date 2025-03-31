terraform {
  required_providers {
    kamatera = {
      source = "Kamatera/kamatera"
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

# Set up private network
resource "kamatera_network" "private-network" {
  name = "${var.environment}-private-network"
  datacenter_id = data.kamatera_datacenter.frankfurt.id
}

# Set up public network
resource "kamatera_network" "public-network" {
  name = "${var.environment}-public-network"
  datacenter_id = data.kamatera_datacenter.frankfurt.id
}

# Provision manager 
resource "kamatera_server" "manager" {
  count             = var.manager_count
  name              = "srv-${var.environment}-manager-${count.index + 1}"
  image_id          = data.kamatera_image.ubuntu.id
  datacenter_id     = data.kamatera_datacenter.frankfurt.id
  cpu_cores         = var.manager_cpu
  cpu_type          = "A"
  ram_mb            = var.manager_ram
  disk_sizes_gb     = [ var.manager_disk_size ]
  billing_cycle     = "hourly"
  power_on          = true

  connection {
    type     = "ssh"
    user     = "root"
    password = var.password
    host     = self.network_interfaces_elastic_ips[0]
    timeout  = "5m"
  }

  network {
    name = kamatera_network.public-network.name
  }

  network {
    name = kamatera_network.private-network.name
  }

  # provisioner "scripts" {
  #   inline = var.install_docker_script
  # }
}

# Provision workernode 
resource "kamatera_server" "worker" {
  count             = var.worker_count
  name              = "srv-${var.environment}-worker-${count.index + 1}"
  image_id          = data.kamatera_image.ubuntu.id
  datacenter_id     = data.kamatera_datacenter.frankfurt.id
  cpu_cores         = var.worker_cpu
  cpu_type          = "A"
  ram_mb            = var.worker_ram
  disk_sizes_gb     = [ var.worker_disk_size ]
  billing_cycle     = "hourly"
  power_on          = true

  connection {
    type     = "ssh"
    user     = "root"
    password = var.password
    host     = self.network_interfaces_elastic_ips[0]
    timeout  = "5m"
  }

  # provisioner "scripts" {
  #   inline = var.install_docker_script
  # }
}
