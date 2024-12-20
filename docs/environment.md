# Environment

The environment of the website is designed to ensure high performance, scalability, and security. This setup consists of several core components, including web servers, databases, DNS (Domain Name System) configuration, and associated applications, which all work in unison to deliver a seamless user experience.

## Overview

In modern software development, creating robust applications requires a structured approach to managing various environments throughout the lifecycle of an application. Each environment serves a distinct purpose, from local development on a developer’s laptop to production systems running in a private cloud.

This chapter explores the four key environments — Development, Testing, Staging, and Production. Each environment is tailored to specific tasks, enabling developers and teams to effectively write, test, and deploy code with confidence. By understanding the unique characteristics and roles of each environment, teams can enhance collaboration, streamline workflows, and ensure the stability and reliability of applications in real-world scenarios. This structured approach minimizes risks, improves quality, and facilitates continuous delivery and integration.

Available environments are:
- Development
- Testing
- Staging
- Production

### Development Environment

Location: Local Laptop

This is where developers write and test their code.

### Testing Environment

Location: Private Cloud  

This environment is used for running automated tests and manual quality assurance checks. It closely mimics the production environment to ensure that the application behaves as expected before it is deployed live. Runs in Docker containers using a configuration that reflects the production setup.

### Staging Environment

Location: Private Cloud  

The staging environment acts as a final testing ground before the application goes live. It should replicate the production environment as closely as possible to catch any issues that may not have appeared in the testing phase. Uses the same Docker containers and configurations as production to ensure consistency.

### Production Environment

Location: Private Cloud

This is the live environment where the application is accessible to end-users. Stability and performance are critical here.

## Services Overview

### Traefik 

aka traefik(./svc/traefik.md)

![traefik-architecture](../img/traefik-architecture.png)

Traefik is a modern reverse proxy and load balancer designed to manage and route traffic to microservices and APIs. It dynamically discovers services via integration with popular orchestration systems like Docker, Kubernetes, and Consul, and automatically configures routing based on service changes. Traefik simplifies managing HTTP, HTTPS, and TCP traffic, providing built-in features like SSL termination, load balancing, and request routing. It's particularly suited for microservice architectures, offering seamless integration, auto-discovery, and flexibility, making it a powerful tool for efficiently handling network traffic in dynamic environments.

Clear Responsibilities

- Providers discover the services that live on your infrastructure (their IP, health, ...)
- Entrypoints listen for incoming traffic (ports, ...)
- Routers analyze the requests (host, path, headers, SSL, ...)
- Services forward the request to your services (load balancing, ...)
- Middlewares may update the request or make decisions based on the request (authentication, rate limiting, headers, ...)

### Consul

aka consul

![consul-architecture](img/consul-architecture.png)

Consul is an open-source tool by HashiCorp designed for service discovery, configuration management, and network automation within distributed systems. It helps applications and services communicate seamlessly across diverse environments, such as cloud, on-premises, or hybrid infrastructures. Consul provides key features, including service discovery (to find and connect services dynamically), health checking (to monitor service availability), a distributed key-value store (for configuration), and network segmentation (with service mesh capabilities to secure and manage network traffic). Consul's adaptability makes it valuable for managing microservices and complex, dynamic environments efficiently.

### PostgreSQL

aka postgres

PostgreSQL is a powerful, open-source relational database management system known for its stability, flexibility, and support for advanced data types and operations. It supports both SQL (relational) and JSON (non-relational) querying, making it suitable for a wide range of applications. PostgreSQL is ACID-compliant, ensuring reliable transactions, and includes features such as indexing, full-text search, and support for complex queries and joins. It’s widely used for handling large-scale, high-availability databases, and is known for its extensibility, allowing developers to add custom functions and data types.

### PGAdmin4

aka pgadmin

pgAdmin 4 is a comprehensive web-based management tool for PostgreSQL databases. It provides a user-friendly interface to manage, monitor, and interact with PostgreSQL databases, making it easier to run queries, view database objects, and configure settings. pgAdmin 4 supports features like query tools, graphical query building, data export/import, and server monitoring, allowing database administrators and developers to efficiently handle database tasks. With its ability to manage multiple databases from a single interface, pgAdmin 4 is a popular choice for both small and large PostgreSQL deployments.

### Minio

aka minio

MinIO is an open-source, high-performance, object storage system compatible with the Amazon S3 API. It is designed for large-scale data storage, handling unstructured data such as photos, videos, and backups across distributed systems. MinIO provides high availability and fault tolerance by allowing data to be stored and replicated across multiple nodes. Its scalability, combined with fast read/write operations, makes it ideal for cloud-native applications, machine learning, and big data workloads. MinIO can be deployed on-premises, in hybrid clouds, or as part of containerized environments like Kubernetes.

Minio is used as storage service.

### Mkdocs

aka mkdocs

MkDocs is a fast, simple, and open-source static site generator specifically designed for creating project documentation. Written in Python, it converts Markdown files into a full-featured, customizable documentation website. With its straightforward setup and a variety of themes (like the popular "Material for MkDocs"), MkDocs is ideal for technical documentation, allowing developers to focus on content creation while handling all the styling, navigation, and structure. It’s widely used for project documentation due to its ease of use, clean interface, and seamless integration with version control platforms like GitHub.

## Domain configuration

### Domain setup

The website is set up using a structure that follows the pattern: `{application}.{environment}.{domain}`.

- **`{application}`:** Refers to specific services or components of the site, such as `api`, `admin`, or `app`.
- **`{environment}`:** Represents the environment in which the application is deployed, such as development, test, staging, or production. This allows for different instances of the same application to coexist across environments. For production, the environment is skipped.
- **`{domain}`:** The main domain for the website, like `example.com`.

Example: For an API running in the staging environment, the URL would look like `api.staging.example.com`.

DNS records are configured to map subdomains (e.g., `api.staging.example.com`) to the server or load balancer where the application is hosted. You create CNAME or A records for each application-environment combination. Traefik dynamically manages these routes, handling SSL certificates (via Let's Encrypt) and load balancing as needed. For instance:

- `api.staging.example.com` → CNAME or A record pointing to the server's IP or Traefik's load balancer.
- `admin.example.com` → CNAME or A record pointing to a different IP or Traefik instance handling the production environment.

DNS resolves the requested domain (e.g., `api.staging.example.com`) to the IP where Traefik is running. Traefik inspects the incoming request, checks the hostname, and applies the matching routing rule to direct the request to the corresponding application and environment.

### Domain overview

The table below provides a structured view of the domain configuration for the website. It outlines the various applications (e.g., web, API, admin, blog) across different environments, such as production and staging. Each domain is associated with a fully qualified domain name (FQDN) that is used to access the respective service in its environment.

| Application  | Environment   | Domain                       | Path        | Description                                     |
|--------------|---------------|------------------------------|-------------|-------------------------------------------------|
| Traefik      | Test          | proxy.test.huybrechts.xyz    | /dashboard/ | Application proxy for test environment          |
| PG Admin     | Test          | admin.test.huybrechts.xyz    | /pgadmin    | Database administration service (/pgadmin)      |
| Minio        | Test          | data.test.huybrechts.xyz     | /           | Minio console running in test environment       |
| Website      | Test          | test.huybrechts.xyz          | /           | Main website running in test environment        |
|              |               |                              |             |                                                 |
| Traefik      | Staging       | proxy.staging.huybrechts.xyz | /dashboard/ | Application proxy for staging environment       |
| PG Admin     | Staging       | admin.staging.huybrechts.xyz | /pgadmin    | Database administration service (/pgadmin)      |
| Minio        | Staging       | data.staging.huybrechts.xyz  | /           | Minio console running in staging environment    |
| Website      | Staging       | staging.huybrechts.xyz       | /           | Main website running in staging environment     |
|              |               |                              |             |                                                 |
| Traefik      | Production    | proxy.huybrechts.xyz         | /dashboard/ | Application proxy for production environment    |
| PG Admin     | Production    | admin.huybrechts.xyz         | /pgadmin    | Database administration service (/pgadmin)      |
| Minio        | Production    | data.huybrechts.xyz          | /           | Minio console running in production environment |
| Website      | Production    | huybrechts.xyz               | /           | Main website running in production environment  |

### Domain management

The DNS management table below outlines the configuration of DNS records for the environments. Each subdomain is mapped to its corresponding record type (e.g., A or CNAME) and the target it points to, which could be an IP address or another domain. This setup ensures that each service, such as the main website, API, and admin panel, is correctly routed in both environments.

The table helps illustrate how traffic is managed and directed for different subdomains, allowing for efficient and organized domain resolution across the development and production environments.

| Subdomain                         | Type | Points to        | Description                                        |
|-----------------------------------|------|------------------|----------------------------------------------------|
| test.huybrechts.xyz               | A    | {ip-address}     | Main website running in test environment           |
| www.test.huybrechts.xyz           | A    | {ip-address}     | Main website running in test environment           |
| proxy.test.huybrechts.xyz         | A    | {ip-address}     | Application proxy in test environment              |
| admin.test.huybrechts.xyz         | A    | {ip-address}     | Database administration in test environment        |
| data.test.huybrechts.xyz          | A    | {ip-address}     | Minio administration in test environment           |
|                                   |      |                  |                                                    |
| staging.huybrechts.xyz            | A    | {ip-address}     | Main website running in staging environment        |
| www.staging.huybrechts.xyz        | A    | {ip-address}     | Main website running in staging environment        |
| proxy.staging.huybrechts.xyz      | A    | {ip-address}     | Application proxy in staging environment           |
| admin.staging.huybrechts.xyz      | A    | {ip-address}     | Database administration in staging environment     |
| data.staging.huybrechts.xyz       | A    | {ip-address}     | Minio administration in staging environment        |
|                                   |      |                  |                                                    |
| huybrechts.xyz                    | A    | {ip-address}     | Main website running in production environment     |
| www.huybrechts.xyz                | A    | {ip-address}     | Main website running in production environment     |
| proxy.huybrechts.xyz              | A    | {ip-address}     | Application proxy in production environment        |
| admin.huybrechts.xyz              | A    | {ip-address}     | Database administration in production environment  |
| data.huybrechts.xyz               | A    | {ip-address}     | Minio administration in production environment     |

**Explanation:**

- **Subdomain:** The specific subdomain (or root domain) that the DNS record is managing (e.g., api.example.dev or example.xyz).
- **Record Type:** Defines the type of DNS record. Common types include:
  - **A:** Points to an IP address (e.g., the server's address).
  - **CNAME:** Alias for another domain (e.g., www.example.dev → example.dev).
- **Points to:** The IP address or domain that the DNS record resolves to.
- **Description:** Provides additional context, such as whether it's for production or development, or if it's an API or admin service.

This table helps clarify how the DNS records are structured across the different environments and subdomains for both example.dev (development) and example.xyz (production).

## Infrastructure configuration

### Hardware overview

The configuration of the servers used for testing, staging, and production environments. Each server is hosted on a cloud provider and consists of dedicated compute and storage resources tailored to the specific needs of each environment.

**Overview**

    Provider: Kamatera
    Environments: Testing, Staging, Production
    Server Types: Virtual Private Server (VPS)
    Architecture: All environments follow a consistent setup but vary in hardware resources based on their requirements.

**Common Configuration**

The following configuration is consistent across all environments:

    Operating System: Ubuntu 22.04 LTS
    Reverse Proxy: Traefik (manages HTTP/HTTPS traffic routing)
    Database: PostgreSQL, managed via pgAdmin 4
    Object Storage: MinIO for S3-compatible object storage
    Testing Service: whoami for traffic and service validation
    Networking: Public and private networking configured
        SSH access with private key authentication for secure management.
        Firewall: Configured to allow only necessary ports (e.g., SSH, HTTP, HTTPS).
    Backup and Monitoring: Configured for automatic backups and basic monitoring via Kamatera’s tools.

**Hardware Specifications**

Each environment has dedicated hardware configurations optimized for its intended use. Each server is configured similarly to maintain consistency across environments, with adjustments in compute and storage resources to meet the respective demands. This setup ensures smooth transitions from testing through staging to production, with hardware capacity scaled appropriately for performance and reliability.

### Docker swarm

Docker is a platform that enables developers to automate the deployment, scaling, and management of applications within lightweight, portable containers. These containers encapsulate an application and its dependencies, ensuring that it runs consistently across different environments, whether it's a developer's laptop, a testing server, or a production environment.

Docker Swarm, on the other hand, is a native clustering and orchestration tool for Docker containers. It allows you to manage a group of Docker engines (nodes) as a single virtual system. With Swarm, you can easily deploy, manage, and scale applications across multiple containers and nodes, providing high availability and load balancing. It simplifies the process of orchestrating complex applications composed of multiple services, making it easier to maintain and scale in a production environment.

In a Docker setup, a Docker Compose file is used to define and manage multi-container applications. This file specifies the services, networks, and volumes required for your application, allowing you to easily set up and run different environments (like development, testing, and production) with specific configurations.

> This system uses docker and docker swarm to run the different environments.  
> Each environment has its own docker compose file  
> compose.{environment}.yml

### Application deployment

## Application Structure

The application is organized into a structured directory layout that facilitates efficient management and scalability. At the top level, the `app` directory contains several key subdirectories:

```powershell
  app
  ├── cert +          # Certificates
  ├── data +          # Data directory
  │ ├── consul +      # Consul data
  │ ├── pgadmin +     # PG admin data
  │ └── pgdata +      # PostgreSql databases
  └── logs +          # Logging
```

## Solution configuration

### Environments and Secrets

In modern application development, managing sensitive information such as credentials, API keys, and configuration settings is crucial for maintaining security and integrity. All environment variables (envvars) in our setup are utilized as secrets to protect sensitive data from exposure in code repositories and during runtime. These secrets are securely stored in GitHub environment secrets, ensuring that access is restricted and controlled.

The deployment process is managed through our CI/CD pipeline, which automatically retrieves and injects these secrets into the application as needed. This streamlined approach enhances security by ensuring that sensitive information is not hard-coded into files or directly exposed in the source code.

Below is an overview of all the secrets utilized in the pipeline, as well as those defined in Docker Compose files and application configuration files. They are maintained for each environment.

| Secret Name         | Type   | Description                       | Example                                        |
|---------------------|--------|-----------------------------------|------------------------------------------------|
| `APP_DATA_USERNAME` | Secret | DB Admin                          | `admin1`                                       |
| `APP_DATA_PASSWORD` | Secret | DB Password                       | `1234`                                         |
| `APP_HOST_USERNAME` | Secret | Server username                   | `user1`                                        |
| `APP_HOST_PASSWORD` | Secret | Server password                   | `1234`                                         |
| `APP_HOST_SERVER`   | Secret | Server IP                         | `10.0.0.1`                                     |
| `APP_HOST_PORT`     | Secret | SSH Port                          | `22`                                           |
| `REGISTRY_USERNAME` | Secret | Container registry username       | `user1`                                        |
| `REGISTRY_PASSWORD` | Secret | Container registry password       | `1234`                                         |
| | | | | 
| `APP_DATA_URL`      | Secret | Database connection               | `DS://{username}:{password}@{database}`        |
| `APP_DATA_NAME`     | Secret | Database name                     | `appdata`                                      |
| `APP_DATA_CONTEXT`  | Secret | Select specific connection string | `SqliteContext`                                |
| `APP_HOST_EMAIL`    | Secret | Server email                      | `a@b.com`                                      |
| `APP_AUTH_GOOGLE`   | Secret | JSON with client ID and secret    | `{ ClientId: abc, ClientSecret: 123 }`         |
| `APP_SMTP_OPTIONS`  | Secret | JSON with SMTP server options     | `{ Server: ... }`                              |


### Continuous Integration and Delivery

Continuous Integration (CI) and Continuous Deployment (CD) are essential practices in modern software development that promote rapid and reliable delivery of applications. CI involves the frequent integration of code changes into a shared repository, often facilitated by version control systems like Git. This process allows developers to detect and fix issues early, improving code quality and accelerating the development cycle.

In our workflow, when a code change is merged into the main branch of the Git repository, a CI pipeline is automatically triggered. This pipeline runs a series of tests to ensure that the new changes do not introduce any bugs or regressions, maintaining the stability of the codebase.

In addition to the automated pipeline for testing, we have separate pipelines for deploying to the staging and production environments. However, these pipelines are run manually to ensure that deployments are deliberate and well-coordinated, allowing for additional checks and balances before changes are pushed to these critical environments. This structured approach to CI/CD enables a smoother development process while ensuring the reliability and quality of the application at every stage.

### Automated Environment Deployment

The deployment process for an environment is orchestrated through a series of well-defined jobs within a CI/CD pipeline. When a deployment is triggered, the workflow initiates the following sequence of actions:

1. **Initialization of the Server**: If specified, the pipeline begins by executing the *init-server* job, which connects to the target server via SSH. This job ensures that essential packages, including Docker and Docker Compose, are installed and configured. Additionally, it sets up the firewall rules to enhance security, ensuring that the server is ready for subsequent operations.

2. **Server Update**: The *update-server* job is executed next, updating the server configuration. It creates necessary application directories if they do not already exist and handles Docker secrets management. Secrets, such as database credentials and API keys, are checked and created as Docker secrets to ensure secure access during deployment.

3. **Building the Website**: The *build-website* job compiles the application, restoring dependencies, building the project, and running tests to ensure functionality. Upon successful completion, the Docker image for the website is built and pushed to a Docker registry.

4. **Deploy Generic Configuration**: The *deploy-generic* job follows, where configuration files specific to the environment (e.g., `src/compose.${{ inputs.environment }}.yml`) are transferred to the server using SCP (Secure Copy Protocol). This setup prepares the environment for deployment.

5. **Deployment of Docker Swarm Stack**: Finally, depending on the specified input, either the *deploy-stack-build* or *deploy-stack-nobuild* jobs are executed. These jobs deploy the Docker stack using the configuration file specific to the environment, initiating the application services defined in the Compose file. This ensures that the application is up and running in the desired environment.

Throughout this process, logs are maintained to track deployment activities, helping with troubleshooting and monitoring the health of the application. The entire deployment cycle is structured to ensure reliability, security, and a seamless transition from code changes to a live application environment.
