# Huybrechts-XYZ

Huybrechts-XYZ is a self-hosted `cloud` platform designed to provide centralized authentication, secure service access, and streamlined deployment of custom and third-party applications. The platform enables users to run various services such as personal websites, blogs, and internally developed software with ease and security.

## Objectives for the Platform

- **Centralized Authentication:** Implement Single Sign-On (SSO) using Keycloak for unified user access management across all services.
- **Secure Reverse Proxy:** Use Traefik as a dynamic reverse proxy to manage routing, TLS termination, and authentication enforcement.
- **Service Integration:** Seamlessly integrate both custom applications and standard services behind a secure and manageable interface.
- **Self-Hosting Flexibility:** Maintain complete control over infrastructure and services, supporting rapid deployment and customization.

## Platform Overview

graph TD
    Users[Users (Browsers, Apps)] <---> Traefik[Traefik Proxy<br>(Routing, TLS, OIDC)]
    Traefik --> Keycloak[Keycloak SSO<br>(Authentication Server)]
    Keycloak --> Blog[Blog Site]
    Keycloak --> API[Custom API]
    Keycloak --> Apps[Other Apps]
    Keycloak --> DB[Databases]

## Documentation

- [Infrastructure Provisioning](./infrastructure.md)
- [Infrastructure Deployment](./deployment.md)
- [Platform Deployment](./platform.md)
- [Platform Services](./services.md)

## Repository Overview

The repository is organized to cover all aspects of the self-hosted system, including deployment, configuration, and source code.

- `.backup/`  
  Backup files and snapshots for critical data and configurations.

- `github/workflow/`  
  GitHub Actions workflows for CI/CD automation.

- `deploy/scripts/`  
  Deployment scripts for installing and updating services.

- `deploy/terraform/`  
  Terraform infrastructure-as-code for provisioning servers and cloud resources.

- `design/`  
  System design documentation and architecture diagrams.

- `docs/`  
  User manuals, technical documentation, and maintenance guides.

- `scripts/`  
  Runtime scripts for system maintenance, available for both Windows and Bash environments.

- `src/`  
  Source code and configuration files, organized by individual service.

## TL;DR

### Links

- Github: [huybrechtsxyz](https://github.com/huybrechtsxyz/huybrechtsxyz)
- Kamatera: [Console](https://console.kamatera.com)
- Terraform: [HCP](https://app.terraform.io/app)

## How To

### How to create new service secret

- Bitwarden Secret Manager. Create new secret. Link to Github machine account.
- Go to the `_pipeline.yml` and find the configuration job
- Find the appropriate Bitwarden step
- Add the UUID from Secret Manager and Variable Name
- Add the SECRET_VariableName: ${{ steps.get-secrets.outputs.VariableName }}

Once declared such, the system will propaget the secret into the docker swarm cluster.











Networking

    Domains and subdomains (e.g., auth.example.dev, traefik.example.dev)

    TLS setup (Let’s Encrypt, custom certs)

Security Model

    SSO flows

    OAuth2/OIDC strategy

    Firewall rules / security groups

3️⃣ Deployments

Deployment Model

    Docker Compose / Kubernetes / Ansible / other

Components

    Traefik

    Keycloak

    OAuth2-Proxy

    Any other apps/services

For each:

    Purpose

    Deployment location (which server)

    Environment variables & secrets (reference to vault, not stored here)

    Volumes / persistence

Diagrams

    Show how requests flow between services

4️⃣ Configuration

Traefik

    EntryPoints

    Routers, Services, Middleware

    Dashboard protection (OAuth2-Proxy)

Keycloak

    Realms

    Clients (one per service)

    Roles / Groups

    Identity Providers (if any)

OAuth2-Proxy

    Config reference

    How to add a new client

DNS

    Subdomain mappings

    Wildcard considerations

5️⃣ Scripts

Provisioning

    What scripts set up the environment (e.g., bootstrap scripts)

    How to run them safely

Maintenance

    Renewing certificates

    Backing up Keycloak DB

    Updating Docker images

Examples

    Sample commands (e.g., docker-compose up -d, traefik reload)

6️⃣ Operations & Maintenance

Monitoring

    How to view logs

    Health endpoints

    Alerts (if any)

Backup & Restore

    Keycloak database

    Traefik config

    Any persistent volumes

Updating Components

    Safe upgrade procedures

    Rollback strategy

Secrets Management

    Where secrets are stored (e.g., Vault, .env files)

    How to rotate them

7️⃣ Onboarding & Contribution Guide

Prerequisites

    Accounts

    SSH keys

    Permissions

Running Locally

    How to spin up the stack (e.g., dev mode)

Adding a New Service

    How to:

        Create a Keycloak client

        Configure OAuth2-Proxy

        Add Traefik routing

Code & Config Standards

    Naming conventions

    Review process

8️⃣ Appendices

    Glossary

    External references (Traefik docs, Keycloak docs)

    Known issues & troubleshooting guide

    Changelog