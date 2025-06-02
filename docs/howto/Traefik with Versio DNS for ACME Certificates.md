README: Using Traefik with Versio DNS for ACME Certificates in Docker Swarm
📌 Problem Summary

Setting up Traefik with Docker Swarm to automatically issue Let's Encrypt TLS certificates using the Versio DNS API (via LEGO) fails with:

acme: error: 401 Unauthorized

or

acme: waiting for DNS record propagation

This happens when:

    Traefik is configured to use Versio as the DNS provider

    DNS-01 challenges are used

    Required secrets and API access are not fully set up

✅ Final Goal

Enable Traefik (running in Docker Swarm) to automatically issue and renew wildcard and base certificates like:

    *.abc.dev

    abc.dev

    proxy.abc.dev

Using Versio’s DNS API securely, and ensuring automatic certificate generation and renewal.
🛠️ Components

    Docker Swarm

    Traefik v2

    LEGO DNS Provider: versio

    Let's Encrypt (ACME)

    DNS records managed at Versio.nl

    Secrets passed via Docker Secrets

🔒 Secrets Needed

These are required to authenticate against the Versio API:

    VERSIO_USERNAME

    VERSIO_PASSWORD

They must be available as Docker secrets, not environment variables.
📁 Docker Compose Snippet (Traefik Service)

secrets:
  VERSIO_USERNAME:
    external: true
  VERSIO_PASSWORD:
    external: true

services:
  traefik:
    image: traefik:v2.11
    ports:
      - "80:80"
      - "443:443"
    secrets:
      - VERSIO_USERNAME
      - VERSIO_PASSWORD
    environment:
      VERSIO_USERNAME_FILE: /run/secrets/VERSIO_USERNAME
      VERSIO_PASSWORD_FILE: /run/secrets/VERSIO_PASSWORD
    volumes:
      - /var/run/docker.sock:/var/run/docker.sock
      - traefik_data:/app/data
    command:
      - "--certificatesresolvers.versioResolver.acme.email=webmaster@abc.dev"
      - "--certificatesresolvers.versioResolver.acme.storage=/app/data/acme-versio.json"
      - "--certificatesresolvers.versioResolver.acme.dnschallenge.provider=versio"
      - "--certificatesresolvers.versioResolver.acme.dnschallenge.delayBeforeCheck=10"

🧪 Symptoms and Troubleshooting
❌ Problem: 401 Unauthorized

Cause: Versio API credentials are incorrect or your server IP is not whitelisted.
✅ Fix:
1. Verify credentials

Try authenticating with curl:

curl -u "your_username:your_password" \
  "https://www.versio.nl/api/v1/dns/list?domain=abc.dev"

You should get a valid JSON response.
2. Whitelist the Docker Swarm node’s IP

Login to your Versio API settings and make sure your Swarm manager's public IP is listed under:

IP addresses with access to the API

    🛑 If your IP is not listed, Versio will return 401 Unauthorized no matter what.

📈 Problem: waiting for DNS record propagation
✅ Fix:

This is not an error — Traefik is waiting for the _acme-challenge TXT record to propagate.

    LEGO has created the TXT record via the Versio API

    Let's Encrypt is checking if it's publicly visible

✅ Verify via DNS:

dig TXT _acme-challenge.proxy.abc.dev +short

Or use:
🔗 https://dnschecker.org/#TXT/_acme-challenge.proxy.abc.dev

Once the TXT record appears globally, Let's Encrypt will validate and issue the cert.
🛡️ Required DNS Setup at Versio

For each domain:

    ✅ Add an A record pointing to your Swarm manager IP (e.g. proxy.abc.dev → 1.2.3.4)

    ✅ (Optional) Wildcard A record: *.abc.dev → 1.2.3.4

    ✅ CAA record (recommended):

    Type: CAA
    Name: abc.dev
    Value: 0 issue "letsencrypt.org"

🧼 Optional: Environment Variable Template

You can manage secrets like this before deployment:

cat <<EOF > /tmp/secrets.env
VERSIO_USERNAME=your_user
VERSIO_PASSWORD=your_password
EOF

scp /tmp/secrets.env root@your-server:/tmp/secrets.env

On the server:

while IFS='=' read -r key value; do
  echo "$value" | docker secret create "$key" -
done < /tmp/secrets.env

🧪 Validation

After starting Traefik, you should see logs like:

[INFO] Obtaining certificate for domain proxy.abc.dev
[INFO] Waiting for DNS record propagation
[INFO] Successfully obtained certificate for domain proxy.abc.dev

✅ Summary Checklist
Step	Done?
Traefik configured with LEGO DNS provider for Versio	✅
Docker secrets used for credentials	✅
Versio API IP whitelist includes your host IP	✅
DNS A and CAA records configured in Versio	✅
_acme-challenge TXT record appears when needed	✅
Certificate issuance works	✅