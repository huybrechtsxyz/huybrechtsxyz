# Deployment Overview

The platform uses GitHub Actions workflows to fully automate the infrastructure provisioning, configuration, and deployment of application services.

The deployment process is split into multiple workflows and jobs, organized roughly in this sequence:

1. Provision Servers (Terraform)
2. Parse Outputs & Identify Servers
3. Initialize Docker Swarm Cluster
4. Configure Swarm Nodes
5. Build and Push Docker Images
6. Deploy Services to Swarm

## Triggering the Deployment

There are two main workflows you can trigger manually (workflow_dispatch). You can trigger these workflows in the GitHub Actions UI.

### Deploy Platform

- Provisions all infrastructure resources (infrastructure: true).
- Meant for first-time or full environment setup.

### Deploy Shared

Deploys shared services only (infrastructure: false), skipping infrastructure provisioning.

## Infrastructure Provisioning (Terraform)

Use Terraform to provision cloud infrastructure (e.g., servers, networks).  
Runs when the infrastructure input is true.

- Checkout code.
- Retrieve secrets (API keys, SSH keys) from Bitwarden.
- Set up Terraform CLI.
- Run terraform-apply.sh to create servers in Kamatera.
- Output information about provisioned resources (tf_output.json).
- Store the Terraform outputs as a build artifact and a cache for future runs.

## Restore Cache (Restore)

Retrieve the infrastructure outputs (server details). If no infrastructure is generates this restores the terraform output from the cache.

- Download tf_output.json from artifacts or cache.
- Parse it with jq to extract a matrix of servers (manager and worker nodes).

## Instrastructure Initialization (Initialization)

Initialize Docker Swarm on the new servers and prepares them for use.  
Steps are per server using a matrix strategy.

- Upload the private SSH key.
- Run initialize-swarm-server.sh to;
  - Bootstrap Docker Swarm.
  - Join nodes to the cluster.

This script automates the preparation of a Linux server to join a Docker Swarm cluster. It:

- Updates the system
- Configures SSH keys
- Sets up the firewall
- Mounts disks
- Installs Docker
- Initializes or joins a Docker Swarm
- Enables the Docker service
- Cleans up temporary files

It is designed to be idempotent (safe to re-run), and uses conventions to identify manager and worker nodes based on the hostname.

### Join a Docker Swarm cluster

The core Swarm logic performs the following:

- Checks if the current node is already part of a Swarm:
- If yes, and the node is manager-1, it refreshes join tokens.
- If yes, and the node is any other node, it skips rejoining.

If not already in a Swarm:

- If the hostname contains manager-1, initializes a new Swarm cluster (docker swarm init) and saves join tokens.
- All other nodes:
  - Wait up to 60 seconds for tokens to become available on the manager.
  - Retrieve tokens over SSH.
  - Join the Swarm as either a manager or worker, determined by hostname pattern.

> Swarm join tokens are stored in /tmp/manager_token.txt and /tmp/worker_token.txt.  
  **Do not move these files to /tmp/app.**  
  /tmp/app is cleaned at the end of the script.

The PRIVATE_IP and MANAGER_IP environment variables must be defined in your environment before running.

### Firewall configuration steps

- Opens the necessary ports for Docker Swarm communication:
  - 2377/tcp – Swarm manager control traffic (cluster management)
  - 7946/tcp & 7946/udp – Node discovery and communication
  - 4789/udp – Overlay network traffic between containers
  - Optionally allows SSH (22/tcp) so you can connect remotely

Updates the firewall rules to allow inbound traffic on these ports, ensuring Swarm nodes can see and talk to each other

## Configuration

Configure each Swarm node.
Steps are per server using a matrix strategy.

- Load environment and platform secrets.
- Run configure-swarm-server.sh to
  - Create directories.
  - Configure Docker Swarm environment.
  - Set up credentials and environment variables.

Given a Swarm cluster, this script sets up overlay networks, secrets, node labels, and filesystem layout for services defined in a workspace JSON. On the leader node, it also handles the global Swarm configuration (networks and secrets). It ensures each node is correctly labeled and that all service folders and environment variables are prepared.

This script automates preparing and configuring a Docker Swarm node. Specifically, it:

- Creates overlay networks (WAN/LAN).
- Loads and manages Docker secrets.
- Applies labels to all Swarm nodes (roles, instance identifiers, etc.).
- Sets up workspace directories and service configuration paths based on a JSON definition (workspace.$WORKSPACE.json).
- Runs per-service configuration scripts (configure.sh) if present.

It is designed to be:

- Idempotent (it checks for existence before creating things).
- Multi-node aware (it labels all Swarm nodes).
- Modular (clearly separated functions).

### Main Components and Flow

#### Docker Network Management

- Checks whether a Docker overlay network exists.
- Creates it if not and errors if Swarm mode isn’t active.

#### Docker Secrets Management

Reads a .env-formatted secrets file ($PATH_TEMP/src/secrets.env).  
For each secret:

- Checks if it already exists.
- If in use by any container/service, leaves it alone.
- Otherwise, deletes and recreates it.

#### Node Labeling

For every Swarm node, determines: role (from hostname), instance (e.g., manager-1), and server (role-instance).

- Applies standardized labels:

    > role={role}  
    > {role}=true  
    > server={server}  
    > instance={instance}

- Loads any custom labels from the workspace JSON.
- Removes labels that no longer apply.

#### Workspace Creation

This is the biggest part and sets up the filesystem layout:

- Reads workspace.$WORKSPACE.json.
- Finds the node’s server_id by hostname.
- Figures out the mount point template (e.g., /mnt/${disk}) and substitutes disk identifiers.
- Creates all required directories for services:
- Copies configuration files (*.env, etc.).
- For each service:
  - Sets up directory structure.
  - Optionally chmods permissions.
  - Exports path environment variables to services.env for other scripts to consume.
  - Finally, runs each service’s configure.sh if present.

#### Cleanup

After configuration adjusts permissions (chmod 755) on config files and deletes temporary files.

#### Dependencies

This script requires:

- Docker CLI with Swarm mode (docker swarm init must have been run).
- jq for JSON processing.
- log function (sourced from libutils.sh):
- source /path/to/libutils.sh

## Discover Docker Images

Discover which services need to be built. If rebuild: true, the pipeline builds and pushes Docker images,

- Run docker-generate-matrix.sh to scan the src/ directory for metadata.json.
- Generate a matrix of services to build.

## Building Docker Images

Build and push Docker images for each service that was found.
Steps are build per service using a matrix strategy.

- Checkout code.
- Retrieve Docker registry credentials.
- (If .NET service) Restore, build, and test with dotnet.
- Log in to Docker registry.
- Build and push the image.

## Starting the Environment

Finally, the pipeline will deploy the services stack onto Docker Swarm.

- Set up Traefik ingress.
- Load configuration secrets.
- Start containers across nodes.

## Secrets Management

All secrets (API keys, credentials) are securely pulled from Bitwarden using bitwarden/

When you need to add a new secret (like an API key or credential), follow these steps:

### Create the Secret in Bitwarden

- Log in to your Bitwarden Vault and go to Secret Manager
- Create a new secret entry and link it to the correct machine
- Copy the secret ID (the UUID identifier Bitwarden uses)

### Locate the Correct Workflow Job

In your GitHub Actions workflow YAML (e.g., platform.yml), find the job that requires the secret. This is usually the Configuration job. Inside that job, look for the Bitwarden secret retrieval step. It usually looks like this:

    - name: Retrieve secrets from Bitwarden
      id: get-secrets
      uses: bitwarden/sm-action@v2
      with:
        secrets: |
          <existing secrets>

### Add Your Secret to the Retrieval Step

Under secrets:, add a new line specifying the mapping:

> {bitwarden-secret-id} > {variable-name}

Let’s say you want to create a secret variable called PLATFORM_USERNAME:

```yaml
with:
  secrets: |
    5e0d3f3c-xxxx-xxxx-xxxx-4e5a6b7c8d9e > PLATFORM_USERNAME
    d1e2f3g4-xxxx-xxxx-xxxx-7h8i9j0k1l2m > PLATFORM_PASSWORD

(Replace the IDs with your Bitwarden secret IDs)
```

### Make the Secret Available in the Environment

After retrieving the secret, you must expose it to your scripts by adding it to env: in the step(s) that need it Here’s how to make PLATFORM_USERNAME available as an environment variable:

> It needs to have the prefix SECRET_ for configuration. This will automatically propagate the secret to the docker swarm cluster.

```yaml
- name: Configure the platform
  run: ./configure-platform.sh
  env:
    SECRET_PLATFORM_USERNAME: ${{ steps.get-secrets.outputs.PLATFORM_USERNAME }}
    SECRET_PLATFORM_PASSWORD: ${{ steps.get-secrets.outputs.PLATFORM_PASSWORD }}
```

## Artifacts and Caching

### Artifacts

- `tf_output.json` is saved as an artifact to allow subsequent jobs to access server metadata.

### Cache

- Docker build layers cached for faster rebuilds.
- Terraform output cached to avoid re-provisioning unnecessarily.
