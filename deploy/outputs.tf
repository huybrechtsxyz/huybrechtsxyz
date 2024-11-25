output "manager_ips" {
  value = kamatera_server.manager[*].public_ip
}

output "worker_ips" {
  value = kamatera_server.worker[*].public_ip
}
