# Kamatera API details
variable "api_key" {
  description = "Kamatera API key"
  type        = string
}

variable "api_secret" {
  description = "Kamatera API secret"
  type        = string
}

# Environment and password
variable "environment" {
  description = "Environment to deploy: test, staging, production"
  type        = string
}

variable "password" {
  description = "Root password for the servers"
  type        = string
}

# Manager configuration
variable "manager_count" {
  description = "Number of manager nodes"
  type        = number
}

variable "manager_cpu" {
  description = "Number of CPU cores for manager nodes"
  type        = number
}

variable "manager_ram" {
  description = "Amount of RAM (GB) for manager nodes"
  type        = number
}

variable "manager_disk_size" {
  description = "Disk size (GB) for manager nodes"
  type        = number
}

# Worker configuration
variable "worker_count" {
  description = "Number of worker nodes"
  type        = number
}

variable "worker_cpu" {
  description = "Number of CPU cores for worker nodes"
  type        = number
}

variable "worker_ram" {
  description = "Amount of RAM (GB) for worker nodes"
  type        = number
}

variable "worker_disk_size" {
  description = "Disk size (GB) for worker nodes"
  type        = number
}

# Block storage configuration
# This is equal to the number of workers
variable "block_storage_count" {
  description = "Number of block storage volumes to create"
  type        = number
}

variable "block_storage_size" {
  description = "Size of each block storage volume (GB)"
  type        = number
}
