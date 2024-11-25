# Main Terraform script to set up servers and networks using Kamatera provider

# Provider configuration
provider "kamatera" {
  username = var.kamatera_username
  password = var.kamatera_password
}

# Variables
module "networks" {
  source = "./networks"
}

module "instances" {
  source = "./instances"
}

module "disks" {
  source = "./disks"
}
