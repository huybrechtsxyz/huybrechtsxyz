terraform {
  required_providers {
    kamatera = {
      source = "Kamatera/kamatera"
    }
  }
}

provider "kamatera" {
  api_key    = var.api_key
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
  code = "24.04 64bit_optimized_updated"
}

# Set up private network
resource "kamatera_private_network" "network" {
  name = "${var.environment}-private-network"
  datacenter_id = data.kamatera_datacenter.frankfurt.id
}

# Set up public network
resource "kamatera_public_ip" "lb_ip" {
  name = "${var.environment}-load-balancer-ip"
  datacenter_id = data.kamatera_datacenter.frankfurt.id
}

# Set up public firewall
resource "kamatera_firewall" "lb_firewall" {
  name = "${var.environment}-lb-firewall"
  datacenter_id = data.kamatera_datacenter.frankfurt.id

  rules = [
    {
      port     = "80"          # Allow HTTP
      protocol = "tcp"
      access   = "allow"
      sources  = ["0.0.0.0/0"]
    },
    {
      port     = "443"         # Allow HTTPS
      protocol = "tcp"
      access   = "allow"
      sources  = ["0.0.0.0/0"]
    },
    {
      port     = "2377"        # Docker Swarm manager communication
      protocol = "tcp"
      access   = "allow"
      sources  = ["10.0.0.0/16"]
    },
    {
      port     = "0-65535"     # Deny all other traffic
      protocol = "tcp"
      access   = "deny"
      sources  = ["0.0.0.0/0"]
    }
  ]
}

# Provision manager 
resource "kamatera_server" "manager" {
  count             = var.manager_count
  name              = "srv-${var.environment}-manager-${count.index + 1}"
  image             = data.kamatera_image.ubuntu
  datacenter_id     = data.kamatera_datacenter.frankfurt.id
  cpu               = var.manager_cpu
  ram               = var.manager_ram
  disk_size         = var.manager_disk_size
  network_interfaces = [{
    network = kamatera_private_network.network.id
    ip      = "auto"
  }]
  network_interfaces_elastic_ips = ["auto"]
  power_on          = true

  billing_cycle = "hourly"

  tags = {
    environment = var.environment
    role        = "manager"
  }

  connection {
    type     = "ssh"
    user     = "root"
    password = var.password
    host     = self.network_interfaces_elastic_ips[0]
    timeout  = "5m"
  }

  provisioner "scripts" {
    inline = var.install_docker_script
  }
}

# Provision workernode 
resource "kamatera_server" "worker" {
  count             = var.worker_count
  name              = "srv-${var.environment}-worker-${count.index + 1}"
  image             = data.kamatera_image.ubuntu
  datacenter_id     = data.kamatera_datacenter.frankfurt.id
  cpu               = var.worker_cpu
  ram               = var.worker_ram
  disk_size         = var.worker_disk_size
  network_interfaces = [{
    network = kamatera_private_network.network.id
    ip      = "auto"
  }]
  network_interfaces_elastic_ips = ["auto"]
  power_on          = true

  billing_cycle = "hourly"

  tags = {
    environment = var.environment
    role        = "worker"
  }
  
  connection {
    type     = "ssh"
    user     = "root"
    password = var.password
    host     = self.network_interfaces_elastic_ips[0]
    timeout  = "5m"
  }

  provisioner "scripts" {
    inline = var.install_docker_script
  }
}

# Provision load_balancer 
resource "kamatera_load_balancer" "manager_lb" {
  name           = "${var.environment}-manager-lb"
  datacenter_id  = data.kamatera_datacenter.frankfurt.id
  balancing_type = "round_robin"
  health_check   = {
    protocol = "tcp"
    port     = 2377
  }
  instances      = [
    for manager in kamatera_server.manager : manager.id
  ]
  public_ip      = kamatera_public_ip.lb_ip.ipv4

  firewall_id    = kamatera_firewall.lb_firewall.id

  tags = {
    environment = var.environment
    role        = "load_balancer"
  }
}

# Provision block_storage 
resource "kamatera_block_storage" "block_storage" {
  count          = var.block_storage_count
  name           = "${var.environment}-block-storage-${count.index + 1}"
  datacenter_id  = data.kamatera_datacenter.frankfurt.id
  size           = var.block_storage_size

  # Attach block storage based on worker count
  attached_to = var.worker_count > 0 
    ? kamatera_server.worker[count.index % var.worker_count].id 
    : kamatera_server.manager[count.index % var.manager_count].id

  tags = {
    environment = var.environment
    role        = "block_storage"
  }
}
