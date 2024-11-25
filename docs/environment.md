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

- [x](x) aka x

| Service | Ports | Domain | Path | Description |
|---------|-------|------- | ---- | ----------- |

### Variables overview

| Secret Name         | Type   | Description                       | Example                                        |
|---------------------|--------|-----------------------------------|------------------------------------------------|
| `KAMATERA_API_ID`   | Secret | Kamatera API Key                  | `123456`                                       |
| `KAMATERA_API_KEY`  | Secret | Kamatera API Key                  | `123546`                                       |

### Application overview

The application is organized into a structured directory layout that facilitates efficient management and scalability. At the top level, the `app` directory contains several key subdirectories:

    ```powershell
    app
    ├── x +                # Consul
    │ ├── x +              #   Consul configuration
    │ ├── data +                #   Consul data
    ```

### Domain overview

| Subdomain                         | Type | Points to        | Description                                        |
|-----------------------------------|------|------------------|----------------------------------------------------|
| test.huybrechts.xyz               | A    | {ip-address}     | x           |

