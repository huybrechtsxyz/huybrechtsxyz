https://console.cloud.google.com/auth/clients/1

# 🛡️ SSO Setup with oauth2-proxy, Keycloak, and Google

This guide explains how to configure Single Sign-On in Docker Swarm using oauth2-proxy, Keycloak, and Google login.

---

## ✨ 1️⃣ Adding oauth2-proxy to Docker Compose / Swarm

**Example `docker-compose.yml` service:**

```yaml
services:
  oauth2-proxy:
    image: quay.io/oauth2-proxy/oauth2-proxy:v7.6.0
    networks:
      - traefik
    environment:
      - OIDC_ISSUER_URL=https://<KEYCLOAK_DOMAIN>/realms/<REALM>
      - OAUTH2_CLIENT_ID=<client-id>
      - OAUTH2_CLIENT_SECRET=<client-secret>
      - OAUTH2_COOKIE=<cookie-secret> # 16, 24 or 32 bytes base64 string
      - REDIRECT_HOST=auth.<domain>
      - COOKIE_DOMAIN=.dev.mycompany.local
      - WHITELIST_DOMAIN=.dev.mycompany.local
    command:
      - --provider=oidc
      - --oidc-issuer-url=$(OIDC_ISSUER_URL)
      - --client-id=$(OAUTH2_CLIENT_ID)
      - --client-secret=$(OAUTH2_CLIENT_SECRET)
      - --cookie-secret=$(OAUTH2_COOKIE)
      - --cookie-secure=true
      - --cookie-domains=$(COOKIE_DOMAIN)
      - --redirect-url=https://$(REDIRECT_HOST)/oauth2/callback
      - --scope=openid email profile
      - --session-store-type=cookie
      - --cookie-expire=8h
      - --cookie-refresh=1h
      - --skip-provider-button=true
      - --email-domain=*
    labels:
      - traefik.enable=true
      - traefik.http.routers.oauth2.rule=Host(`auth.<domain>`)
      - traefik.http.routers.oauth2.entrypoints=websecure
      - traefik.http.routers.oauth2.tls.certresolver=<resolver>
      - traefik.http.services.oauth2.loadbalancer.server.port=4180

Note:

    Replace <KEYCLOAK_DOMAIN>, <REALM>, <client-id>, <client-secret>, and <domain> with your actual values.

🛡️ 2️⃣ Creating a Keycloak Client for oauth2-proxy

In the Keycloak Admin Console:

    Select your realm (e.g., myrealm).

    Go to Clients.

    Click Create.

        Client ID: oauth2-proxy

        Client Protocol: openid-connect

        Root URL: https://auth.<domain>

    Save.

⚙️ 3️⃣ Configuring the Keycloak Client

In your created client:

    Access Type: confidential

    Standard Flow Enabled: ON

    Direct Access Grants Enabled: OFF

    Service Accounts Enabled: OFF

    Valid Redirect URIs:

https://auth.<domain>/oauth2/callback

(or wildcard: https://auth.<domain>/oauth2/*)

Web Origins:

    +

    Credentials: Copy Client Secret to use in oauth2-proxy.

✅ Save your changes.
🌐 4️⃣ Configuring Google Developer Console

    Create OAuth 2.0 Client:

        Go to Google Cloud Console.

        APIs & Services → Credentials.

        Create Credentials → OAuth Client ID.

        Application Type: Web application.

        Authorized Redirect URIs:

        https://<keycloak-domain>/realms/<realm>/broker/google/endpoint

    Copy Client ID and Client Secret.

🔗 5️⃣ Adding Google as Identity Provider in Keycloak

In Keycloak Admin Console:

    Go to Identity Providers.

    Click Add provider → Google.

    Fill:

        Alias: google

        Client ID: (from Google)

        Client Secret: (from Google)

        Default Scopes: openid email profile

        Trust Email: ON

    Save.

✅ You now have Google login enabled in Keycloak.
✨ 6️⃣ (Optional) Auto-redirect to Google

If you want to skip the Keycloak login page:

    Add the query parameter to the authorization request:

    kc_idp_hint=google

    In oauth2-proxy config:

        Use --skip-provider-button=true.

        Set the --login-url accordingly if needed (advanced).

⚠️ 7️⃣ Notes & Tips

    You should never visit auth.<domain> directly—instead, access your protected app.

    If you visit auth.<domain> with no path, you will get a 404—this is normal.

    Forward Auth in Traefik:

        Each protected app router must have a forward auth middleware pointing to oauth2-proxy.

Example forward auth label:

- traefik.http.middlewares.forward-auth.forwardauth.address=https://auth.<domain>/oauth2/auth
- traefik.http.middlewares.forward-auth.forwardauth.trustForwardHeader=true

✅ Done! You now have:

    Traefik protecting your services.

    oauth2-proxy handling authentication.

    Keycloak as the OIDC provider.

    Google login integrated into Keycloak.