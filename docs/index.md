# Documentation

The Huybrechts XYZ platform is an application by Vincent Huybrechts.

- Traefik for reverse proxy

- Consul for health and config

- PostgreSQL for SQL
- PGAdmin for SQL Admin

- Redis for Caching
- RedisInsight for Redis Admin

- Keycloak for IAM
  - Depends on: Postgres
  - Depends on: Postgress=init script for creating schema

---------------------------

WORKING DIRECTORIES FOR APP

---------------------------

| Path | Type | Base Path | Purpose |
|------|------|-----------|---------|
| conf | /etc/app/{service}/ | Configuration | files |

data	/var/lib/app/{service}/	Internal state, databases, etc.
logs	/var/log/app/{service}/	Logs
public	/srv/app/{service}/	User-facing or served content



Recommended Filesystem Strategy for Docker Swarm
1. Configuration Files

    Store in:
    /etc/app/{service}/
    or
    a GitOps-style repo mounted into containers as read-only volumes.

2. Logs

    Store in:
    /var/log/app/{service}/
    or use a centralized logging system (e.g., Fluentd, Loki, ELK).

3. App Data (Volumes)

    Store in:
    /var/lib/app/{service}/
    or
    /srv/app/{service}/ if the data is user-facing (e.g., SeaweedFS blobs).

    Backed by:
    /sbd/var/lib/app/{service}/ via bind mounts or named volumes.
