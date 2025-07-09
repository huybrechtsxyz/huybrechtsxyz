SSO Setup with oauth2-proxy, Keycloak, and Google on Docker Swarm

This guide walks you through configuring Single Sign-On (SSO) with oauth2-proxy, Keycloak, and Google login, running on Docker Swarm with Traefik as the ingress proxy.

It also covers how to restrict access based on Keycloak group membership mapped to a realm role.
1️⃣ Adding oauth2-proxy to Docker Compose / Swarm

Sample docker-compose.yml snippet:

services:
  oauth2-proxy:
    image: quay.io/oauth2-proxy/oauth2-proxy:v7.6.0-alpine
    networks:
      - traefik
    environment:
      - OIDC_ISSUER_URL=https://<KEYCLOAK_DOMAIN>/realms/<REALM>
      - OAUTH2_CLIENT_ID=<client-id>
      - OAUTH2_CLIENT_SECRET=<client-secret>
      - OAUTH2_COOKIE=<cookie-secret> # 16, 24 or 32 bytes base64 string
      - REDIRECT_HOST=auth.<domain>
      - COOKIE_DOMAIN=.yourdomain.dev
      - WHITELIST_DOMAIN=.yourdomain.dev
      - OAUTH2_PROXY_ALLOWED_GROUPS=/sysadmins
    command:
      - --provider=oidc
      - --oidc-issuer-url=$(OIDC_ISSUER_URL)
      - --client-id=$(OAUTH2_CLIENT_ID)
      - --client-secret=$(OAUTH2_CLIENT_SECRET)
      - --cookie-secret=$(OAUTH2_COOKIE)
      - --cookie-secure=true
      - --cookie-domains=$(COOKIE_DOMAIN)
      - --redirect-url=https://$(REDIRECT_HOST)/oauth2/callback
      - --scope=openid email profile groups
      - --session-store-type=cookie
      - --cookie-expire=8h
      - --cookie-refresh=1h
      - --skip-provider-button=true
      - --email-domain=*
      - --request-logging=true
      - --insecure-oidc-skip-issuer-verification # Optional for self-signed certs
    labels:
      - traefik.enable=true
      - traefik.http.routers.oauth2.rule=Host(`auth.<domain>`)
      - traefik.http.routers.oauth2.entrypoints=websecure
      - traefik.http.routers.oauth2.tls.certresolver=<resolver>
      - traefik.http.services.oauth2.loadbalancer.server.port=4180

    Notes:

        Replace <KEYCLOAK_DOMAIN>, <REALM>, <client-id>, <client-secret>, <cookie-secret>, <domain>, and <resolver> with your real values.

        OAUTH2_PROXY_ALLOWED_GROUPS restricts access to users in the /sysadmins group (see Keycloak setup).

2️⃣ Create and Configure Keycloak Client for oauth2-proxy
In Keycloak Admin Console:

    Go to your realm (e.g., myrealm).

    Navigate to Clients → Create.

    Enter:

        Client ID: oauth2-proxy

        Client Protocol: openid-connect

        Root URL: https://auth.<domain>

    Save.

Client Settings:

    Access Type: confidential

    Standard Flow Enabled: ON

    Direct Access Grants Enabled: OFF

    Service Accounts Enabled: OFF

    Valid Redirect URIs:

https://auth.<domain>/oauth2/callback

or wildcard:

https://auth.<domain>/oauth2/*

Web Origins:

    +

    Credentials: Copy Client Secret to use in oauth2-proxy.

Add Group Membership Mapper:

    Go to Client Scopes or Mappers tab.

    Add a mapper:

        Mapper Type: Group Membership

        Token Claim Name: groups

        Full group path: checked (recommended)

        Add to ID token and access token.

3️⃣ Add Google as an Identity Provider in Keycloak
In Google Cloud Console:

    Navigate to APIs & Services → Credentials.

    Create OAuth 2.0 Client ID:

        Application Type: Web application

        Authorized Redirect URIs:

        https://<keycloak-domain>/realms/<realm>/broker/google/endpoint

    Copy Client ID and Client Secret.

In Keycloak:

    Go to Identity Providers.

    Select Google.

    Configure:

        Alias: google

        Client ID: (from Google)

        Client Secret: (from Google)

        Default Scopes: openid email profile

        Trust Email: ON

    Save.

4️⃣ (Optional) Auto-redirect Users to Google Login

To skip Keycloak’s login page and directly redirect to Google:

    Add query parameter:

kc_idp_hint=google

In oauth2-proxy, use:

    --skip-provider-button=true

    Adjust --login-url if needed for advanced cases.

5️⃣ Restrict Access by Keycloak Group Membership (Realm Role Mapping)
In Keycloak:

    Create a realm role called sysadmins.

    Create a group named sysadmins.

    Assign the sysadmins role to the sysadmins group.

    Add users to the sysadmins group to grant them this role.

In oauth2-proxy config:

Add or set:

allowed_groups = "/sysadmins"
scope = "openid email profile groups"

    Important: The /sysadmins value is the full group path as it appears in the token. You can verify the exact string by decoding the token at jwt.io.

6️⃣ Traefik Forward Authentication Setup

For each protected service, add a Traefik middleware to forward auth requests to oauth2-proxy.

Example Traefik labels on your service:

labels:
  - traefik.http.middlewares.forward-auth.forwardauth.address=https://auth.<domain>/oauth2/auth
  - traefik.http.middlewares.forward-auth.forwardauth.trustForwardHeader=true
  - traefik.http.routers.myapp.middlewares=forward-auth

7️⃣ Troubleshooting
Symptom	Cause	Solution
invalid_client unauthorized error	Client ID or secret mismatch	Verify client credentials in Keycloak and proxy
403 Forbidden after login	User not in allowed group	Confirm user group membership in Keycloak
Redirect URI mismatch	Redirect URI does not match	Match exact URI in Keycloak and oauth2-proxy config
Missing groups claim	Group Membership mapper not configured	Add and enable Group Membership mapper
404 at auth.<domain>	Direct visit to oauth2-proxy root	Access protected services instead
8️⃣ Verifying Tokens

    Login to your app via oauth2-proxy.

    Grab the id_token from browser dev tools or logs.

    Paste the token payload at https://jwt.io.

    Check the groups claim includes /sysadmins.

Summary

    Keycloak manages authentication and group/role mappings.

    Google login is integrated via Keycloak as an IdP.

    oauth2-proxy enforces OAuth2 authentication and group-based authorization.

    Traefik forwards protected requests to oauth2-proxy for auth.

    Access is restricted to users in the /sysadmins group.


    