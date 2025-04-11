#Expose important values, such as the load balancer's public IP and Elastic IPs.

output "manager_ips" {
  value = [for s in kamatera_server.manager : s.public_ips]
}

output "worker_ips" {
  value = [for s in kamatera_server.worker : s.public_ips]
}
