# Environment Setup

## Introduction

This documentation provides a comprehensive guide to the environment configurations and setups used in the project. It outlines the various types of environments, the services they support, and the associated configurations to ensure a seamless development, testing, and production workflow.

The goal of this document is to:

- Help developers and team members understand the architecture of the environments.
- Provide clear instructions for setting up and using the environments effectively.
- Facilitate troubleshooting and debugging by offering detailed insights into the services and their dependencies.

Whether you are setting up a local development instance, deploying to staging, or managing the production environment, this documentation serves as your go-to resource for environment-related information.

### Service overview

The table below provides a structured view of the domain configuration for the website. It outlines the various applications (e.g., web, API, admin, blog) across different environments, such as production and staging. Each domain is associated with a fully qualified domain name (FQDN) that is used to access the respective service in its environment.

An overview of the used services:

- [Consul](./services/consul.md) aka consul for discovery and configuration
- [Minio](./services/minio.md) aka minio for block storage
- [Prometheus](./services/prometheus.md) aka prometheus for monitoring
- [Thanos](./services/thanos.md) aka thanos for storing prometheus data
- [Traefik](./services/traefik.md) aka traefik as reverse proxy

#### Production services

| Service    | Ports | Domain         | Path        | Description |
|------------|-------|----------------|-------------| ----------- |
| Traefik    |    80 | domain         | /           | Traefik HTTP + Redirect |
| Traefik    |   433 | domain         | /           | Traefik HTTPS |
| Traefik    |   433 | proxy.domain   | /dashboard  | Traefik dasboard |
| Consul     |  8500 | config.domain  | /           | Consul discovery and configuration |
| Prometheus |  9090 | /              | /           | Prometheus monitoring |
| Minio      |  9001 | data.domain    | /           | Minio data storage |
| Thanos     |  9091 | /              | /           | Thanos query 
| Thanos     | 10901 | /
| Thanos     | 10902 | /

#### Development services

| Service    | Ports | Domain          | Path        | Description |
|------------|-------|-----------------|-------------|-------------|
| Traefik    |    80 | localhost       | /           | Traefik HTTP |
| Traefik    |   433 | localhost       | /           | Traefik HTTPS |
| Traefik    |  8080 | proxy.localhost | /dashboard/ | Traefik Dashboard |
| Consul     |  8500 | localhost       | /           | Consul discovery and configuration
| Prometheus |  9090 | localhost       | /           | Prometheus Dashboard |
| Minio      |  9001 | localhost       | /           | Minio data storage |

### Variables overview

| Secret Name           | Type   | Description               | Example  |
|-----------------------|--------|---------------------------|----------|
| `CONSUL_ADDR`         | Env    | Consul server address     | `http://path/to.link:8500'  |
| `MINIO_ROOT_USER`     | Secret | Minio Username            | `user1`  |
| `MINIO_ROOT_PASSWORD` | Secret | Minio Password            | `pass1`  |
| `MINIO_REGION`        | Secret | Minio Username            | `user1`  |
| `KAMATERA_API_KEY`    | Secret | Kamatera API Key          | `123456` |
| `KAMATERA_API_SECRET` | Secret | Kamatera API Key          | `123546` |
| `VERSIO_USERNAME`     | Secret | Versio Username           | `user1`  |
| `VERSIO_PASSWORD`     | Secret | Versio Password           | `pass1`  |
| `VERSIO_ENDPOINT`     | Secret | Versio Endpoint           | `http://path/to.link' |
| `APP_HOST_USERNAME`   | Secret | Server username           | `user1`  |
| `APP_HOST_PASSWORD`   | Secret | Server password           | `1234`   |
| `APP_HOST_SERVER`     | Secret | Server IP                 | `10.0.0.1` |
| `APP_HOST_PORT`       | Secret | SSH Port                  | `22`     |

### Application overview

The application is organized into a structured directory layout that facilitates efficient management and scalability. At the top level, the `app` directory contains several key subdirectories:

    ```powershell
    app
    ├── consul +                 # Consul
    │ ├── conf +                 #   Consul conf
    │ ├── data +                 #   Consul data
    ├── minio +                  # Minio
    │ ├── conf +                 #   Minio conf
    │ ├── data +                 #   Minio data
    ├── prometheus +             # Prometheus
    │ ├── conf +                 #   Prometheus conf
    │ ├── data +                 #   Prometheus data
    ├── traefik +                # Traefik
    │ ├── cert +                 #   Traefik certificates
    │ ├── logs +                 #   Traefik logs
    ```

### Domain overview

| Subdomain                         | Type | Points to        | Description |
|-----------------------------------|------|------------------|-------------|
| test.huybrechts.xyz               | A    | {ip-address}     | The Huybrechts-XYZ landing page |
| test.huybrechts.dev               | A    | {ip-address}     | The Huybrechts-DEV project system page |
| test.meeus.family                 | A    | {ip-address}     | The Meeus.Family landing page |
| test.theorderoftheblacklizard.be  | A    | {ip-address}     | The Black Lizard landing page |
