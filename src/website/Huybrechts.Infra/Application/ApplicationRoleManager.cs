using Huybrechts.Core.Application;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Huybrechts.Infra.Application;

public class ApplicationRoleManager : RoleManager<ApplicationRole>
{
    public ApplicationRoleManager(
        IRoleStore<ApplicationRole> store, 
        IEnumerable<IRoleValidator<ApplicationRole>> roleValidators, 
        ILookupNormalizer keyNormalizer, IdentityErrorDescriber errors, 
        ILogger<RoleManager<ApplicationRole>> logger) 
        : base(store, roleValidators, keyNormalizer, errors, logger)
    {

    }

    public IQueryable<ApplicationTenant> Tenants => ((ApplicationRoleStore)Store).Tenants;

    public override Task<IdentityResult> CreateAsync(ApplicationRole role)
    {
        if (role is not null)
        {
            role.TenantId = ApplicationRole.GetTenantId(role.Id);
            role.Label = ApplicationRole.GetRoleLabel(role.Id);
        }
        return base.CreateAsync(role!);
    }

    public override Task<IdentityResult> UpdateAsync(ApplicationRole role)
    {
        if (role is not null)
        {
            role.TenantId = ApplicationRole.GetTenantId(role.Id);
            role.Label = ApplicationRole.GetRoleLabel(role.Id);
        }
        return base.UpdateAsync(role!);
    }

    public override Task UpdateNormalizedRoleNameAsync(ApplicationRole role)
    {
        if (role is not null)
        {
            role.TenantId = ApplicationRole.GetTenantId(role.Id);
            role.Label = ApplicationRole.GetRoleLabel(role.Id);
        }
        return base.UpdateNormalizedRoleNameAsync(role!);
    }
}
