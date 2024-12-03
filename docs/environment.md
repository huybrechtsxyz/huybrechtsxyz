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
- [Loki](./services/loki.md) aka loki for logging aggretation
- [PGAdmin4](./services/pgadmin.md) aka pgadmin for Sql Management
- [PostgreSql](./services/postgres.md) aka postgres for Sql database
- [Prometheus](./services/prometheus.md) aka prometheus for monitoring
- [Thanos](./services/thanos.md) aka thanos for storing prometheus data
- [Traefik](./services/traefik.md) aka traefik as reverse proxy

The service domains, paths, and ports:

| Service    | Ports | Domain         | Path        | Description |
|------------|-------|----------------|-------------| ----------- |
| Traefik    |    80 | domain         | /           | Traefik HTTP + Redirect |
| Traefik    |   433 | domain         | /           | Traefik HTTPS |
| Traefik    |   433 | proxy.domain   | /dashboard  | Traefik dasboard |
| Consul     |  8500 | config.domain  | /           | Consul configuration |
| Consul     |  8600 | config.domain  | /           | Consul DNS discovery |
| Prometheus |  9090 | /              | /           | Prometheus monitoring |x
| Minio      |  9001 | data.domain    | /           | Minio data storage |x
| Thanos     |  9091 | /              | /           | Thanos query  |x
| Thanos     | 10901 | /              | /           | Thanos sidecar |x
| Thanos     | 10902 | /              | /           | Thanos gateway |x
| Loki       |  3100 | /              | /           | Loki logging |x
| Postgres   |  5432 | /              | /           | PostgreSql server |x
| PGAdmin    |  8880 | /              | /pgadmin    | PostgreSql management |x

### Variables overview

| Secret Name           | Type   | Description               | Example  |
|-----------------------|--------|---------------------------|----------|
| `CONSUL_ADDR`         | Env    | Consul server address     | `http://path/to.link:8500`  |
| `MINIO_ROOT_USER`     | Secret | Minio Username            | `user1`  |x
| `MINIO_ROOT_PASSWORD` | Secret | Minio Password            | `pass1`  |x
| `MINIO_REGION`        | Secret | Minio Username            | `user1`  |x
| `KAMATERA_API_KEY`    | Secret | Kamatera API Key          | `123456` |x
| `KAMATERA_API_SECRET` | Secret | Kamatera API Key          | `123546` |x
| `POSTGRES_DB`         | Secret | Postgres DB name          | `xyzdb`  |x
| `POSTGRES_USER`       | Secret | Postgres Username         | `user1`  |x
| `POSTGRES_PASSWORD`   | Secret | Postgres Password         | `pass1`  |x
| `VERSIO_USERNAME`     | Secret | Versio Username           | `user1`  |
| `VERSIO_PASSWORD`     | Secret | Versio Password           | `pass1`  |
| `VERSIO_ENDPOINT`     | Secret | Versio Endpoint           | `http://path/to.link` |

| Secret Name           | Type   | Description               | Example  |
|-----------------------|--------|---------------------------|----------|
| `APP_HOST_USERNAME`   | Secret | Server username           | `user1`  |x
| `APP_HOST_PASSWORD`   | Secret | Server password           | `1234`   |x
| `APP_HOST_SERVER`     | Secret | Server IP                 | `10.0.0.1` |x

### Application overview

The application is organized into a structured directory layout that facilitates efficient management and scalability. At the top level, the `app` directory contains several key subdirectories:

    ```powershell
    app
    ├── consul +                 # Consul
    │ ├── conf +                 #   Consul conf
    │ ├── data +                 #   Consul data
    x├── minio +                  # Minio
    x│ ├── conf +                 #   Minio conf
    x│ ├── data +                 #   Minio data
    x├── loki +                   # Loki
    x│ ├── conf +                 #   Loki conf
    x│ ├── data +                 #   Loki data
    x├── postgres +               # PostgreSql
    x│ ├── data +                 #   PostgreSql conf
    x│ ├── pgadmin +              #   PostgreSql data
    x├── prometheus +             # Prometheus
    x│ ├── conf +                 #   Prometheus conf
    x│ ├── data +                 #   Prometheus data
    x├── thanos +                 # Thanos
    x│ ├── conf +                 #   Thanos conf
    x│ ├── data +                 #   Thanos data
    ├── traefik +                # Traefik
    │ ├── conf +                 #   Traefik configuration
    │ ├── data +                 #   Traefik certificates
    │ ├── logs +                 #   Traefik logs
    ```

### Domain overview

| Subdomain                         | Type | Points to        | Description |
|-----------------------------------|------|------------------|-------------|
| test.huybrechts.xyz               | A    | {ip-address}     | The Huybrechts-XYZ landing page |
| test.huybrechts.dev               | A    | {ip-address}     | The Huybrechts-DEV project system page |
| test.meeus.family                 | A    | {ip-address}     | The Meeus.Family landing page |
| test.theorderoftheblacklizard.be  | A    | {ip-address}     | The Black Lizard landing page |
