# Traefik

aka traefik

![traefik-architecture](../img/traefik-architecture.png)

Traefik is a modern reverse proxy and load balancer designed to manage and route traffic to microservices and APIs. It dynamically discovers services via integration with popular orchestration systems like Docker, Kubernetes, and Consul, and automatically configures routing based on service changes. Traefik simplifies managing HTTP, HTTPS, and TCP traffic, providing built-in features like SSL termination, load balancing, and request routing. It's particularly suited for microservice architectures, offering seamless integration, auto-discovery, and flexibility, making it a powerful tool for efficiently handling network traffic in dynamic environments.

Clear Responsibilities

- Providers discover the services that live on your infrastructure (their IP, health, ...)
- Entrypoints listen for incoming traffic (ports, ...)
- Routers analyze the requests (host, path, headers, SSL, ...)
- Services forward the request to your services (load balancing, ...)
- Middlewares may update the request or make decisions based on the request (authentication, rate limiting, headers, ...)
