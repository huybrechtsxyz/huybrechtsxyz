# [Vault](./svc/vault.md)

## What is Vault?
Vault is a tool for securely accessing secrets. A secret is anything that you want to tightly control access to, such as API keys, passwords, or certificates. Secure, store and tightly control access to tokens, passwords, certificates, encryption keys for protecting secrets and other sensitive data using a UI, CLI, or HTTP API.

The key features of Vault are:
- Secure Secret Storage: Arbitrary key/value secrets can be stored in Vault. Vault encrypts these secrets prior to writing them to persistent storage, so gaining access to the raw storage isn't enough to access your secrets. Vault can write to disk, Consul, and more.
- Dynamic Secrets: Vault can generate secrets on-demand for some systems, such as AWS or SQL databases. For example, when an application needs to access an S3 bucket, it asks Vault for credentials, and Vault will generate an AWS key-pair with valid permissions on demand. After creating these dynamic secrets, Vault will also automatically revoke them after the lease is up.
- Data Encryption: Vault can encrypt and decrypt data without storing it. This allows security teams to define encryption parameters and developers to store encrypted data in a location such as SQL without having to design their own encryption methods.
- Leasing and Renewal: All secrets in Vault have a lease associated with them. At the end of the lease, Vault will automatically revoke that secret. Clients are able to renew leases via built-in renew APIs.
- Revocation: Vault has built-in support for secret revocation. Vault can revoke not only single secrets, but a tree of secrets, for example all secrets read by a specific user, or all secrets of a particular type. Revocation assists in key rolling as well as locking down systems in the case of an intrusion.

## Glossary
Before describing the architecture, we provide a glossary of terms to help clarify what is being discussed:

- Storage Backend - A storage backend is responsible for durable storage of encrypted data.
- Barrier - The barrier is cryptographic steel and concrete around the Vault.
- Secrets Engine - A secrets engine is responsible for managing secrets.
- Audit Device - An audit device is responsible for managing audit logs.
- Auth Method - An auth method is used to authenticate users or applications which are connecting to Vault. Once authenticated, the auth method returns the list of applicable policies which should be applied.
- Client Token - A client token (aka "Vault Token") is conceptually similar to a session cookie on a web site. Once a user authenticates, Vault returns a client token which is used for future requests.
- Secret - A secret is the term for anything returned by Vault which contains confidential or cryptographic material.
- Server - Vault depends on a long-running instance which operates as a server.

## Architecture of Vault
Once started, the Vault is in a sealed state. Before any operation can be performed on the Vault it must be unsealed. This is done by providing the unseal keys. When the Vault is initialized it generates an encryption key which is used to protect all the data. That key is protected by a master key. After the Vault is unsealed, requests can be processed from the HTTP API to the Core. The core is used to manage the flow of requests through the system, enforce ACLs, and ensure audit logging is done. When a client first connects to Vault, it needs to authenticate.

![Vault Architecture](../img/vault-architecture.png)

## "Dev" Server Mode
You can start Vault as a server in "dev" mode like so: "vault server -dev". This dev-mode server requires no further setup, and your local vault CLI will be authenticated to talk to it. Every feature of Vault is available in "dev" mode. The -dev flag just short-circuits a lot of setup to insecure defaults.

The properties of the dev server (some can be overridden with command line flags or by specifying a configuration file):
- Initialized and unsealed - The server will be automatically initialized and unsealed. It is ready for use immediately.
- In-memory storage - All data is stored (encrypted) in-memory. Vault server doesn't require any file permissions.
- Bound to local address without TLS - The server is listening on 127.0.0.1:8200 (the default server address) without TLS.
- Automatically Authenticated - The server stores your root access token so vault CLI access is ready to go. If you are accessing Vault via the API, you'll need to authenticate using the token printed out.
- Single unseal key - The server is initialized with a single unseal key. The Vault is already unsealed, but if you want to experiment with seal/unseal, then only the single outputted key is required.
- Key Value store mounted - A v2 KV secret engine is mounted at secret/.

