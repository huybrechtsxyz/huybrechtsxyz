# Infrastructure Overview

## Infrastructure Provisioning

The infrastructure for this self-hosted platform is created and maintained using **Terraform**, an infrastructure-as-code tool that enables declarative provisioning and management of cloud resources.

- **Supported Provider:**  
  Currently, only **Kamatera** cloud platform is supported for infrastructure deployment.  
  Kamatera supports only servers (CPU, RAM, disks) and network resources.

- **HCP Terraform:**
  Cloud environment for storing the state for Terraform.

- **Workspace Configuration:**  
  Each deployment workspace is described by a JSON configuration file located in the `/deploy` directory.  
  The file is named using the pattern `workspace,{workspaceid}.json`.

## What is a Workspace?

A workspace is an instance of the full cloud environment. It defines all infrastructure components, network topologies, and storage layouts required to deploy, operate, and manage your services. Workspaces are defined at provisioning and deployment. Workspaces are configured using the workspace configuration file.

Key characteristics of a workspace:

**Composition:**

- A workspace is made up of a set of servers declared in the configuration file.
- Each server has a defined role (e.g., manager, infra, worker) and disk layout (OS disks, data disks).
- Servers are logically organized to support different service types, storage requirements, and workloads.

**Internal Networking:**

- All servers are automatically connected to each other via a private internal VLAN on 10.0.0.0.
- This internal network allows services to communicate securely without relying on public internet routing.

**Public IP Addresses:**

- Kamatera requires each server to be assigned a public IP address (this is a provider constraint).
- However, for security and operational consistency, **only the manager node’s public IP** should be used for administrative access.
- Other servers should be accessed internally whenever possible.

**Purpose:**

- A workspace represents a self-contained environment where all platform services run together.
- You can think of it as a mini-cloud, fully owned and operated by the owner, encapsulating:
  - Compute resources
  - Networking
  - Persistent storage
  - Service configurations
  - Security policies

**Lifecycle:**

- You can create, destroy, and recreate workspaces independently.
- Each workspace can be versioned (e.g., 1.0.0) to track changes to its topology over time.

## Workspace Configuration File Structure

The workspace JSON file defines the overall deployment environment, including service paths and server specifications. The current deployment workspace is designed to host and run various services and applications in a self-managed environment. Here’s a breakdown of its main sections with the current example.

### Workspace Overview: deploy

Metadata about the workspace such as its name, version, and description.

Current setup:

- **Name:** Platform Workspace  
- **Version:** 1.0.0  
- **Description:** Workspace deployment configuration for the shared Platform.

### Paths Configuration: paths

Specifies the types of data storage used by services and their corresponding directory paths, using template variables for dynamic substitution.

> ⚠️ If new types of paths are required by a service, they **must be explicitly added here**. This list defines all valid mount types available across the infrastructure.

Each service uses standardized directory paths for different data types:

| Type    | Path Template           | Purpose                         |
|---------|-------------------------|---------------------------------|
| config  | `/${service}/config`    | Configuration files             |
| data    | `/${service}/data`      | Persistent application data     |
| logs    | `/${service}/logs`      | Service-generated logs          |
| serve   | `/${service}/serve`     | Content served by the services  |

These paths are mounted onto servers' disks according to the mount configuration described below.

### Server Nodes: servers

An array of server definitions describing the nodes within the workspace. Each server entry contains:

- `id`:
  - Unique identifier for the server.
  - Follows naming: srv-{role}-{instance}-{random4}
- `role`:
  - The server's role (e.g., manager, infra, worker).
  - **Manager node:** Typically responsible for orchestrating and managing workloads or cluster control tasks.
  - **Infra nodes:** Provide infrastructure services for other services.
  - **Worker nodes:** Handle execution of application workloads or services.
  - Other types of nodes can be added.
- `labels`:
  - Optional tags for additional metadata (e.g., services running on that server).
- `disks`:
  - An array of attached storage disks with their sizes (in GB) and labels.
  - Each disk is mounted under `/mnt/data{disk}/app` where `${disk}` corresponds to the disk index (1 or 2).
  
- `mountpoint`:
  - The base path where disks are mounted, supporting disk number substitution.
  - The service data types (`config`, `data`, `logs`, `serve`) are mapped to specific disks based on the `mounts` array to separate storage concerns.
  - **Disk Device Mapping:** Disk indices (`disk 1`, `disk 2`) are assumed to map to actual system devices in order: `disk 1` → `/dev/sda`, `disk 2` → `/dev/sdb`, etc.
  - **Mountpoint Template:** The `${disk}` variable in the mount point path (e.g., `/mnt/data${disk}/app`) dynamically maps each data type to the correct physical disk using the `mounts` array.
- `mounts`:
  - Defines which disk is used for each type of data path (`config`, `data`, `logs`, `serve`).
  - **Mounts Purpose:** The `mounts` section for each server allows individual data types (e.g., config, data, logs, serve) to be written to distinct physical disks, providing separation of concerns and better disk utilization or backup strategies.

The platform consists of multiple server nodes with different roles and storage configurations:

| Server ID  | Role    | Labels            | Disks                                | Mount Point Template        | Mounts (Data type → Disk)               |
|------------|---------|-------------------|------------------------------------|----------------------------|----------------------------------------|
| manager-1  | manager | —                 | 1 × 20 GB (manager-1-os)           | `/mnt/data${disk}/app`      | config → disk 1, data → disk 1, logs → disk 1, serve → disk 1 |
| infra-1    | infra   | -                | 1 × 20 GB (infra-1-os), 1 × 40 GB (infra-1-data) | `/mnt/data${disk}/app` | config → disk 2, data → disk 2, logs → disk 2, serve → disk 2 |
| infra-2    | infra   | —                 | 1 × 20 GB (infra-2-os), 1 × 40 GB (infra-2-data) | `/mnt/data${disk}/app` | config → disk 2, data → disk 2, logs → disk 2, serve → disk 2 |
| worker-1   | worker  | —                 | 1 × 20 GB (worker-1-os)            | `/mnt/data${disk}/app`      | config → disk 1, data → disk 1, logs → disk 1, serve → disk 1 |
| worker-2   | worker  | —                 | 1 × 20 GB (worker-2-os)            | `/mnt/data${disk}/app`      | config → disk 1, data → disk 1, logs → disk 1, serve → disk 1 |

This structured configuration allows Terraform scripts to automate the provisioning of servers and storage according to predefined roles and capacity requirements, ensuring consistent and repeatable infrastructure deployments.

This setup allows clear separation of roles and data on distinct nodes with explicit storage configurations, ensuring a scalable and maintainable platform deployment.

## Terraform Infrastructure Provisioning

This project uses Terraform to create, manage, and destroy infrastructure resources on Kamatera. All servers, networks, and related configurations are fully declarative, enabling reproducible, versioned deployments.

Below is an overview of how the Terraform configuration works.

### Directory Structure

- `locals.tf` Defines local variables and computed structures.

- `main.template.tf` The main Terraform configuration (templated with environment variables via envsubst to main.tf).

- `variables.tf` Declares required variables.

-`output.tf` Defines outputs to collect provisioned resource details.

- `vars-{workspace}.tfvars` – Per-workspace variables (e.g., vars-platform.tfvars).

### Core Components

#### Locals

File: locals.tf

The locals block generates the list of servers to provision dynamically from the server_roles map:

    Each role (manager, infra, worker) defines:

        count: how many servers of this role to create

        cpu_cores, cpu_type, ram_mb: hardware specs

        disks_gb: list of disk sizes (root disk + optional data disks)

        unit_cost: cost reference

    flatten() produces a single list of all servers with consistent metadata.

#### Main Template

File: main.template.tf

This is the primary Terraform configuration. It uses:

    terraform { cloud { ... } }

        Stores Terraform state remotely in your Terraform Cloud organization (huybrechts-xyz).

        Names the workspace dynamically: huybrechts-xyz-$WORKSPACE.

    Providers

        kamatera: For managing Kamatera resources.

        random: To generate suffixes for resource naming.

    Resources

        kamatera_network: A private VLAN per workspace.

            Example: vlan-platform-xxxx

            Subnet 10.0.0.0/23

        kamatera_server: All servers defined in local.servers.

            Each server:

                Gets a predictable name, e.g., srv-platform-infra-1-xxxx

                Is attached to:

                    The public WAN network (required by Kamatera)

                    The private VLAN

                Uses Ubuntu 24.04 as the OS.

                Boots with your SSH key and root password.

    Note: The main.template.tf file is processed via envsubst to inject variables from .env files before running Terraform.

#### Outputs

File: output.tf

Outputs expose structured metadata about your servers after creation:

    Server role, index, label.

    Public and private IPs.

    The private IP of the manager-1 node (manager_ip) for internal reference.

This allows your deployment scripts or Ansible to dynamically pick up IPs after provisioning.

#### Variables

File: variables.tf

Declares all required inputs.

API credentials:

    api_key, api_secret

Access credentials:

    ssh_public_key

    password

Workspace/environment settings:

    workspace (e.g., platform)

    environment (e.g., shared)

Server roles:

    A map of all roles and hardware specs.

### How it Works

Lifecycle:

    Generate main.tf:

        Use envsubst to replace variables.

        Example:

    WORKSPACE=platform envsubst < main.template.tf > main.tf

Init:

    terraform init

Plan:

    terraform plan -var-file=vars-platform.tfvars

Apply:

    terraform apply -var-file=vars-platform.tfvars

Outputs:

    Terraform displays server IPs and metadata.

### Important Notes

Private VLAN:

    All servers are connected internally via the VLAN (10.0.0.0/23).
    Always prefer using private IPs for inter-server communication.

Public IPs:

    Every server has a public IP (Kamatera requirement), but only the manager node’s public IP should be used for SSH and public endpoints.

Server Disks:

    You can attach multiple disks by specifying multiple sizes in disks_gb.
    Example: [20, 40] creates a 20GB OS disk + 40GB data disk.

Dynamic Naming:

    Server names are unique and predictable:
    srv-{workspace}-{role}-{index}-{random suffix}

## How to SSH into a Server

- Go to [`console.kamatera.com`](https://console.kamatera.com).
- Go to Servers, and select a server.
- Choose Connect to server

## How to Add a New Server

### 1. Decide the role name

Choose a role (e.g., infra, worker, manager, or create a new one).
Roles are logical groups of servers with the same configuration.

### 2. Edit the variables file

Open your workspace variable file, e.g.:

    /deploy/terraform/vars-platform.tfvars

Add or modify the role definition:

``` json
server_roles = {
  worker = {
    count     = 2   # <-- Increase this to 3 to add one more worker
    cpu_type  = "A"
    cpu_cores = 1
    ram_mb    = 2048
    disks_gb  = [20]
    unit_cost = 6.00
  }
}

Tip: To add a completely new role, create a new block:

server_roles = {
  extra = {
    count     = 1
    cpu_type  = "A"
    cpu_cores = 2
    ram_mb    = 4096
    disks_gb  = [20]
    unit_cost = 10.00
  }
}
```

### 3. Re-apply Server Terraform

Terraform will create the new server(s).

    terraform plan -var-file=vars-platform.tfvars
    terraform apply -var-file=vars-platform.tfvars

Or run the platform deployment script

## How to Add a Path Type

Paths define where data, config, logs, or served files are stored per service.

### 1. Edit the workspace JSON

Example:

    /deploy/workspace,platform.json

Find the paths array and add your new path type:

```json
"paths": [
  { "type": "config", "path": "/${service}/config" },
  { "type": "data",   "path": "/${service}/data" },
  { "type": "logs",   "path": "/${service}/logs" },
  { "type": "serve",  "path": "/${service}/serve" },
  { "type": "cache",  "path": "/${service}/cache" }
]
```

### 2. Update any scripts

If you have deployment scripts or volume mounts, update them to handle the new path type (cache in this example).

- Make sure you update the mounts for each server in the workspace configuration.

## How to Add an Extra Disk to a Server Role

### 1. Edit the variables file

Find the relevant server role in your variables file, e.g.:

```json
infra = {
  count     = 2
  cpu_type  = "A"
  cpu_cores = 1
  ram_mb    = 4096
  disks_gb  = [20,40]
  unit_cost = 11.00
}
```

Add another disk size to disks_gb:

disks_gb = [20,40,50]  # Adds a new 50GB disk to all infra servers

### 2. Update the mount configuration

If needed, update mounts in the workspace file to mount paths to the new disk index:

"mounts": [
  { "type": "config", "disk": 2 },
  { "type": "data", "disk": 2 },
  { "type": "logs", "disk": 3 },   // Now logs go to disk 3
  { "type": "serve", "disk": 2 }
]

### 3. Re-apply Mount Terraform

terraform plan -var-file=vars-platform.tfvars  
terraform apply -var-file=vars-platform.tfvars  
Or return the platform deployment pipeline

## How to Create a New Workspace (Environment)

Each workspace represents a separate environment.

### 1. Create a workspace JSON

Duplicate an existing workspace file:

    cp deploy/workspace,platform.json deploy/workspace,newenv.json

Adjust the settings as needed.

### 2. Create a variables file

Duplicate the vars-platform.tfvars:

    cp deploy/terraform/vars-platform.tfvars deploy/terraform/vars-newenv.tfvars

Edit:

    workspace = "newenv"
    environment = "staging"

### 3. Plan and Apply Terraform

    terraform workspace new newenv  
    terraform init
    terraform plan -var-file=vars-newenv.tfvars
    terraform apply -var-file=vars-newenv.tfvars

Or run the platform deployment pipeline.

## How to Change Hardware Specs

Edit the server_roles definition in your variables file:

    cpu_cores

    cpu_type

    ram_mb

    disks_gb

Example:

```json
worker = {
  count     = 2
  cpu_type  = "A"
  cpu_cores = 2
  ram_mb    = 4096
  disks_gb  = [40]
  unit_cost = 12.00
}
```

Then re-run:

terraform plan -var-file=vars-platform.tfvars  
terraform apply -var-file=vars-platform.tfvars  
Or run the platform deployment pipeline.
