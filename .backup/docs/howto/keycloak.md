
realm roles

    - system admin

users
    - Add role system-admin if applicable

Keycloak OIDC
Config Options
Flag	Toml Field	Type	Description	Default
--allowed-role	allowed_roles	string | list	restrict logins to users with this role (may be given multiple times). Only works with the keycloak-oidc provider.	
Usage

    see docker compose for oauth2proxy

Keycloak new admin console (default as of v19.0.0)

The following example shows how to create a simple OIDC client using the new Keycloak admin2 console. However, for best practices, it is recommended to consult the Keycloak documentation.

The OIDC client must be configured with an audience mapper to include the client's name in the aud claim of the JWT token.
The aud claim specifies the intended recipient of the token, and OAuth2 Proxy expects a match against the values of either --client-id or --oidc-extra-audience.

In Keycloak, claims are added to JWT tokens through the use of mappers at either the realm level using "client scopes" or through "dedicated" client mappers.

Creating the client

    Create a new OIDC client in your Keycloak realm by navigating to:
    Clients -> Create client
        Client Type 'OpenID Connect'
        Client ID <your client's id>, please complete the remaining fields as appropriate and click Next.
            Client authentication 'On'
            Authentication flow
                Standard flow 'selected'
                Direct access grants 'deselect'
                    Save the configuration.
            Settings / Access settings:
                Valid redirect URIs 
                    https://{service}.{domain-dev}/oauth2/callback
                    https://{service}.{domain-dev}/oauth2/callback/*
                    Save the configuration.
                web origins
                    https://traefik.huybrechts.dev
            Under the Credentials tab you will now be able to locate <your client's secret>.
    Configure a dedicated audience mapper for your client by navigating to Clients -> <your client's id> -> Client scopes.

    Access the dedicated mappers pane by clicking <your client's id>-dedicated, located under Assigned client scope.
    (It should have a description of "Dedicated scope and mappers for this client")
        Click Configure a new mapper and select Audience
            Name 'aud-mapper-<your client's id>'
            Included Client Audience select <your client's id> from the dropdown.
                OAuth2 proxy can be set up to pass both the access and ID JWT tokens to your upstream services. If you require additional audience entries, you can use the Included Custom Audience field in addition to the "Included Client Audience" dropdown. Note that the "aud" claim of a JWT token should be limited and only specify its intended recipients.
            Add to ID token 'On'
            Add to access token 'On' - #1916
                Save the configuration.
    Any subsequent dedicated client mappers can be defined by clicking Dedicated scopes -> Add mapper -> By configuration -> Select mapper

You should now be able to create a test user in Keycloak and get access to the OAuth2 Proxy instance, make sure to set an email address matching <yourcompany.com> and select Email verified.

Authorization

OAuth2 Proxy will perform authorization by requiring a valid user, this authorization can be extended to take into account a user's membership in realm roles, and client roles using the keycloak-oidc provider options
--allowed-role

Roles

A standard Keycloak installation comes with the required mappers for realm roles and client roles through the pre-defined client scope "roles". This ensures that any roles assigned to a user are included in the JWT tokens when using an OIDC client that has the "Full scope allowed" feature activated, the feature is enabled by default.

Creating a realm role

    Navigate to Realm roles -> Create role
        Role name, <realm role name> -> save

Creating a client role

    Navigate to Clients -> <your client's id> -> Roles -> Create role
        Role name, <client role name> -> save

Assign a role to a user

Users -> Username -> Role mapping -> Assign role -> filter by roles or clients and select -> Assign.

Keycloak "realm roles" can be authorized using the --allowed-role=<realm role name> option, while "client roles" can be evaluated using --allowed-role=<your client's id>:<client role name>.

You may limit the realm roles included in the JWT tokens for any given client by navigating to:
Clients -> <your client's id> -> Client scopes -> <your client's id>-dedicated -> Scope
Disabling Full scope allowed activates the Assign role option, allowing you to select which roles, if assigned to a user, will be included in the user's JWT tokens. This can be useful when a user has many associated roles, and you want to reduce the size and impact of the JWT token.

Tip

To check if roles or groups are added to JWT tokens, you can preview a users token in the Keycloak console by following these steps: Clients -> <your client's id> -> Client scopes -> Evaluate.
Select a realm user and optional scope parameters such as groups, and generate the JSON representation of an access or id token to examine its contents.