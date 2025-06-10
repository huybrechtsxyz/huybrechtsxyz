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