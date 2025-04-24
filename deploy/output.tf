#Expose important values, such as the load balancer's public IP and Elastic IPs.

output "manager_public_ips" {
  value = [for s in kamatera_server.manager : s.public_ips[0]]
}

output "manager_private_ips" {
  value = [for s in kamatera_server.manager : s.private_ips[0]]
}

output "worker_public_ips" {
  value = [for s in kamatera_server.worker : s.public_ips[0]]
}

output "worker_private_ips" {
  value = [for s in kamatera_server.worker : s.private_ips[0]]
}
