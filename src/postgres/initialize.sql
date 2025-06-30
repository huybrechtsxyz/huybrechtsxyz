ALTER SYSTEM SET password_encryption = 'md5';
SELECT pg_reload_conf();

-- Keycloak PostgreSQL schema creation script

-- Create schema for authentik
CREATE SCHEMA IF NOT EXISTS keycloak;
