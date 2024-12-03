# Keycloak

## Introduction to Keycloak

Keycloak is an open-source Identity and Access Management (IAM) solution developed by Red Hat. It provides centralized authentication and authorization services for applications and services, helping to secure modern applications with minimal effort. Keycloak supports features such as single sign-on (SSO), identity brokering, social login, and user federation, and integrates seamlessly with existing identity management systems. It can be used with various protocols like OAuth2, OpenID Connect, and SAML 2.0 to authenticate and authorize users.

Keycloak allows administrators to manage users, roles, and permissions through a comprehensive admin console, making it a popular choice for enterprise-level applications requiring robust security.

## How to Add Users and Configure Them in Keycloak

### Step 1: Access the Keycloak Admin Console

    Start by logging into the Keycloak Admin Console. The URL is typically http://<keycloak-server>:8080/auth (or https://<keycloak-server>:8080/auth if using HTTPS).
    Log in using the admin credentials (or whatever admin credentials you set up when configuring Keycloak).

### Step 2: Create a New User

    In the Keycloak Admin Console, navigate to the "Users" section under the realm you are managing.
    Click on the "Add user" button to create a new user.
    Provide a username and, optionally, a first name, last name, and email. You can also choose whether the user should be enabled immediately.
    Click "Save".

### Step 3: Configure User Credentials

    After creating the user, go to the "Credentials" tab.
    Set a password for the user, and choose whether to force the user to reset their password upon first login.
    Click "Set password" to save the credentials.

### Step 4: Assign Roles to the User

    Navigate to the "Roles" tab to assign roles to the user. Roles in Keycloak determine what resources the user can access and which actions they can perform.
    Select one or more roles and assign them to the user.

### Step 5: Manage User Attributes (Optional)

    Keycloak allows adding custom attributes to users. Under the "Attributes" tab, you can add custom attributes that may be used by your applications.

## Multi-Tenant Support within a Single Realm

In Keycloak, tenants can be implemented using groups and roles within a single realm. A realm in Keycloak acts as a namespace for authentication and authorization data, while groups and roles can be used to organize users into different teams or tenants.

A tenant, in this case, is similar to a team in applications like Microsoft Teams. Instead of creating multiple Keycloak realms (which is typically used for creating completely isolated environments), we can use a single realm to manage multiple tenants (teams) with proper access control.

### Step 1: Define Teams (Tenants) as Groups

    In the Keycloak Admin Console, navigate to the "Groups" section.
    Click "New group" to create a new group. For example, create a group called teamA for one tenant and teamB for another tenant.
    You can add members to each group by going to the "Members" tab within the group and adding users.

### Step 2: Define Roles for Each Tenant

    Navigate to the "Roles" section under the realm settings.
    Create roles that are specific to each tenant (e.g., teamA-admin, teamA-user, teamB-admin, teamB-user).
    Assign these roles to users based on the team they belong to. You can do this by editing the user’s role mappings.

### Step 3: Assign Users to Teams (Tenants)

    Go to the "Users" section in the Keycloak Admin Console.
    Select a user and navigate to the "Groups" tab.
    Add the user to the appropriate group (e.g., teamA or teamB) based on their team/tenant affiliation.
    You can also assign specific roles to the user in the "Role Mappings" tab, ensuring they have access to the correct resources within their team.

### Step 4: Enforce Access Control Based on Tenant

    Use Keycloak's Authorization Services to define access policies and permissions specific to each tenant. These policies can be based on roles, groups, or other attributes.
    Configure applications to check for these roles and groups to determine the level of access the user has within the context of their tenant.

### Benefits of Multi-Tenant Support in a Single Realm:

    Simplified Administration: Managing multiple tenants within a single realm is easier than maintaining multiple realms. You can manage users, groups, roles, and policies centrally.
    Cost Efficiency: Using a single realm reduces the overhead of maintaining multiple Keycloak instances.
    Granular Access Control: Groups and roles within a realm provide a fine-grained way to control access and isolate tenants from each other.

This multi-tenant approach is ideal for scenarios where tenants (e.g., teams, departments, or clients) need to be isolated in terms of access but can share common authentication and authorization infrastructure.