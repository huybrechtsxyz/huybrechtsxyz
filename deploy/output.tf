#Expose important values, such as the load balancer's public IP and Elastic IPs.
output "manager_elastic_ips" {
  description = "Elastic IPs of the Docker Swarm manager nodes"
  value       = kamatera_server.manager[*].network_interfaces_elastic_ips
}

output "worker_elastic_ips" {
  description = "Elastic IPs of the Docker Swarm worker nodes"
  value       = kamatera_server.worker[*].network_interfaces_elastic_ips
}

output "manager_lb_public_ip" {
  description = "Public IP of the load balancer for manager nodes"
  value       = kamatera_load_balancer.manager_lb.public_ip
}

output "block_storage_details" {
  description = "Details of block storage volumes"
  value = [
    for bs in kamatera_block_storage.block_storage : {
      id   = bs.id
      name = bs.name
      size = bs.size
      attached_to = bs.attached_to
    }
  ]
}
