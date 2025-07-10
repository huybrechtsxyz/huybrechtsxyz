# Huybrechts.[dev|xyz]

## Hi there 👋

Huybrechts-XYZ is a self-hosted platform by Vincent Huybrechts designed to provide centralized authentication, secure service access, and streamlined deployment of custom and third-party applications. The platform enables the user to run various services such as personal websites, blogs, and internally developed software with ease and security.

## Sections

- [Repository Overview](./docs/repository.md)

## Getting started

### General Requirements

Ensure your system includes the following:

- Git for repository cloning.
- An IDE like Visual Studio Code.
- Docker desktop or containerd runtime.
- An internet connection to download the container images.

### Cloning Sources

To obtain the source code, use the following command:

```bash
git clone https://github.com/huybrechtsxyz/huybrechtsxyz.git
```

### Repository Overview
The repository is organized to cover all aspects of the self-hosted system, including deployment, configuration, and source code.

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
