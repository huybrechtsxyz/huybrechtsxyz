# Kamatera terraform for Staging environment
# https://kamatera.github.io/kamateratoolbox/serverconfiggen.html?configformat=terraform

terraform {
  required_providers {
    kamatera = {
      source = "Kamatera/kamatera"
    }
  }
}

provider "kamatera" {
}

resource "kamatera_server" "VMSTAGINGWEB01" {
  name = "my_server"
  datacenter_id = "EU-FR"
  image_id = "6000C293f92e9a0c758033a11976d225"
  cpu_type = "A"
  cpu_cores = 1
  ram_mb = 2048
  disk_sizes_gb = [20]
  billing_cycle = "hourly"
}