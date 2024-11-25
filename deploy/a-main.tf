# Network Configuration
resource "kamatera_network" "main_network" {
  name = "main-network"
  cidr = "192.168.1.0/24"  # Change this CIDR block as needed
}

# Virtual Machine Configuration for Manager Node
resource "kamatera_virtual_machine" "manager" {
  name       = "manager-node"
  image_id   = "ubuntu-20.04"  # Change this to the appropriate image ID from Kamatera
  cpu        = 2
  ram        = 4096
  disk_size  = 20
  vlan_id    = kamatera_network.main_network.vlan_id
  ip_address = true
  ssh_key    = var.ssh_key  # SSH Key for access
}

# Virtual Machine Configuration for Worker Node
resource "kamatera_virtual_machine" "worker1" {
  name       = "worker-node-1"
  image_id   = "ubuntu-20.04"  # Change this to the appropriate image ID from Kamatera
  cpu        = 2
  ram        = 4096
  disk_size  = 20
  vlan_id    = kamatera_network.main_network.vlan_id
  ip_address = true
  ssh_key    = var.ssh_key  # SSH Key for access
}

# Define SSH key variable for secure access
variable "ssh_key" {
  description = "SSH key for access to the virtual machines"
  type        = string
}

# Kamatera API Key
variable "kamatera_api_key" {
  description = "Kamatera API Key"
  type        = string
}

# Provider configuration for Kamatera
provider "kamatera" {
  api_key = var.kamatera_api_key
}

# Outputs for VM IP addresses (useful for Docker Swarm join)
output "manager_ip" {
  value = kamatera_virtual_machine.manager.ip_address
}

output "worker1_ip" {
  value = kamatera_virtual_machine.worker1.ip_address
}
