# Output the server details
# Name example: srv-shared-manager-1-1234
# Name example: srv-shared-infra-2-1234
# Name example: srv-shared-worker-3-1234
output "serverdata" {
  value = [
    for key, srv in kamatera_server.server : {
      role       = split("-", key)[2]
      label      = "${split("-", key)[2]}_${tonumber(split("-", key)[3]) + 1}"
      name       = srv.name
      ip         = srv.public_ips[0]
      private_ip = srv.private_ips[0]
      manager_ip = values(kamatera_server.server)[0].private_ips[0]
    }
  ]
}
