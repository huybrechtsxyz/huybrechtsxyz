using Finbuckle.MultiTenant;
using Huybrechts.Core.Application;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Huybrechts.App.Web;

/* Program.cs

    builder.Services.AddSingleton<IAuthorizationHandler, MultiTenantRoleAuthorizationHandler>();

    builder.Services.AddAuthorization(options =>
    {
        options.AddPolicy("IsOwner", policy => policy.Requirements.Add(new HasTenantRoleRequirement(ApplicationRoleTenant.Owner)));
        options.AddPolicy("IsManager", policy => policy.Requirements.Add(new HasTenantRoleRequirement(ApplicationRoleTenant.Manager)));
        options.AddPolicy("IsContributer", policy => policy.Requirements.Add(new HasTenantRoleRequirement(ApplicationRoleTenant.Contributer)));
        options.AddPolicy("IsMember", policy => policy.Requirements.Add(new HasTenantRoleRequirement(ApplicationRoleTenant.Member)));
        options.AddPolicy("IsGuest", policy => policy.Requirements.Add(new HasTenantRoleRequirement(ApplicationRoleTenant.Guest)));
    });


 */

public static class TenantPolicies
{
    public const string IsOwner = "IsOwner";
    public const string IsManager = "IsManager";
    public const string IsContributor = "IsContributor";
    public const string IsMember = "IsMember";
    public const string IsGuest = "IsGuest";
}

public class HasTenantRoleRequirement : IAuthorizationRequirement
{
    public HasTenantRoleRequirement(ApplicationTenantRole role)
    {
        Role = role;
    }

    public ApplicationTenantRole Role { get; set; }
}

public class MultiTenantRoleAuthorizationHandler : AuthorizationHandler<HasTenantRoleRequirement>
{
    public MultiTenantRoleAuthorizationHandler()
    {
    }

    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, HasTenantRoleRequirement requirement)
    {
        bool IsInRole(ClaimsPrincipal user, string tenant, ApplicationTenantRole role)
        {
            return user.IsInRole(ApplicationRole.GetRoleName(tenant, role));
        }

        Task EvaluateRole(AuthorizationHandlerContext context, HasTenantRoleRequirement requirement, bool isInRole)
        {
            if (isInRole)
                context.Succeed(requirement);
            else
                context.Fail(new AuthorizationFailureReason(this, "User not in role"));
            return Task.CompletedTask;
        }

        if (context.Resource is not HttpContext httpContext)
        {
            context.Fail(new AuthorizationFailureReason(this, "Invalid context resource"));
            return Task.CompletedTask;
        }

        TenantInfo? tenantInfo = httpContext.GetMultiTenantContext<TenantInfo>()?.TenantInfo;
        if (tenantInfo is null || string.IsNullOrEmpty(tenantInfo.Identifier))
        {
            context.Fail(new AuthorizationFailureReason(this, "Unable to retrieve tenant info"));
            return Task.CompletedTask;
        }

        List<ApplicationTenantRole> allowedRoles = requirement.Role switch
        {
            ApplicationTenantRole.Guest =>
            [
                ApplicationTenantRole.Guest,
                ApplicationTenantRole.Member,
                ApplicationTenantRole.Contributor,
                ApplicationTenantRole.Manager,
                ApplicationTenantRole.Owner
            ],
            ApplicationTenantRole.Member =>
            [
                ApplicationTenantRole.Member,
                ApplicationTenantRole.Contributor,
                ApplicationTenantRole.Manager,
                ApplicationTenantRole.Owner
            ],
            ApplicationTenantRole.Contributor =>
            [
                ApplicationTenantRole.Contributor,
                ApplicationTenantRole.Manager,
                ApplicationTenantRole.Owner
            ],
            ApplicationTenantRole.Manager => 
            [
                ApplicationTenantRole.Manager, 
                ApplicationTenantRole.Owner
            ],
            ApplicationTenantRole.Owner => 
            [
                ApplicationTenantRole.Owner
            ],
            _ => throw new NotImplementedException()
        };

        var isInRole = allowedRoles.Any(role => IsInRole(context.User, tenantInfo.Identifier, role));
        return EvaluateRole(context, requirement, isInRole);
    }
}