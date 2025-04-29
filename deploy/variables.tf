#
# THESE VARIABLES ARE REQUIRED FOR THE DEPLOYMENT
#

# Kamatera API details
variable "api_key" {
  description = "Kamatera API key"
  type        = string
}

variable "api_secret" {
  description = "Kamatera API secret"
  type        = string
}

# SSH key for SSH access to the server
variable "ssh_public_key" {
  description = "The public SSH key to use for access"
  type        = string
}

#
# THESE VARIABLES ARE COVERED BY THE TERRAFORM VARIABLES FILE
#

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
  validation {
    condition     = var.manager_ram % 1024 == 0
    error_message = "Manager RAM must be a multiple of 1024 MB."
  }
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
  validation {
    condition     = var.worker_ram % 1024 == 0
    error_message = "Worker RAM must be a multiple of 1024 MB."
  }
}

variable "worker_disk_size" {
  description = "Disk size (GB) for worker nodes"
  type        = number
}

variable "block_storage_size" {
  description = "Size of each block storage volume (GB)"
  type        = number
}
