variable "environment" {
  description = "The environment type (test, staging, production)"
  type        = string
}

variable "kamatera_username" {
  description = "Kamatera API username"
  type        = string
}

variable "kamatera_password" {
  description = "Kamatera API password"
  type        = string
}

variable "manager_count" {
  description = "Number of manager nodes"
  type        = number
}

variable "worker_count" {
  description = "Number of worker nodes"
  type        = number
}

variable "ram" {
  description = "RAM for each server"
  type        = number
  default     = 2
}

variable "vcpu" {
  description = "vCPU for each server"
  type        = number
  default     = 1
}

variable "storage_size" {
  description = "Base storage size for each server"
  type        = number
}

variable "disk_size" {
  description = "Size of additional disk for MinIO"
  type        = number
}
