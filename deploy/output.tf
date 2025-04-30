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

output "outputdata" {
  value = {
    include = concat(
      [
        for i in range(length(kamatera_server.manager)) : {
          role       = "manager"
          label      = "manager_${i+1}"
          name       = kamatera_server.manager[i].name
          ip         = kamatera_server.manager[i].public_ips[0]
          private_ip = kamatera_server.manager[i].private_ips[0]
          manager_ip = kamatera_server.manager[i].public_ips[0]
        }
      ],
      [
        for i in range(length(kamatera_server.worker)) : {
          role       = "worker"
          label      = "worker_${i+1}"
          name       = kamatera_server.worker[i].name
          ip         = kamatera_server.worker[i].public_ips[0]
          private_ip = kamatera_server.worker[i].private_ips[0]
          manager_ip = kamatera_server.manager[i].public_ips[0]
        }
      ]
    )
  }
}