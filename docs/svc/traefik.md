# [Traefik](https://traefik.io)

## What is Traefik
Traefik is an open-source Edge Router that makes publishing your services a fun and easy experience. It receives requests on behalf of your system and finds out which components are responsible for handling them.

## Architecture of Consul
![Consul Architecture](../img/traefik-architecture.png)

Clear Responsibilities
- Providers discover the services that live on your infrastructure (their IP, health, ...)
- Entrypoints listen for incoming traffic (ports, ...)
- Routers analyse the requests (host, path, headers, SSL, ...)
- Services forward the request to your services (load balancing, ...)
- Middlewares may update the request or make decisions based on the request (authentication, rate limiting, headers, ...)
