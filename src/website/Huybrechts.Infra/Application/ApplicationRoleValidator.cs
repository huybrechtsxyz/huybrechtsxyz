using Huybrechts.Core.Application;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Huybrechts.Infra.Application;

public class ApplicationRoleValidator : IRoleValidator<ApplicationRole>
{
    public async Task<IdentityResult> ValidateAsync(RoleManager<ApplicationRole> manager, ApplicationRole role)
    {
        ArgumentNullException.ThrowIfNull(manager, nameof(manager));
        ArgumentNullException.ThrowIfNull(role, nameof(role));

        var errors = new List<string>();

        if (!string.IsNullOrWhiteSpace(role.Name))
        {
            var existingRole = await manager.Roles.FirstOrDefaultAsync(x => x.Name == role.Name);
            if (existingRole is null)
            {
                //if (role.IsTenantRole())
                //{
                //    var existingTenant = await (manager as ApplicationRoleManager)!.Tenants.Where(q => q.Id == role.TenantId).FirstOrDefaultAsync();
                //    if (existingTenant is not null)
                //        return IdentityResult.Success;
                //    errors.Add(string.Format("{0} has invalid tenant.", role.Name));
                //}
                //else
                return IdentityResult.Success;
            }
            errors.Add(string.Format("{0} is already taken.", role.Name));
        }
        else
        {
            errors.Add("Name cannot be null or empty.");
        }

        if (errors.Count == 0)
            return IdentityResult.Success;

        IdentityError[] identityErrors = new IdentityError[errors.Count];
        errors.ToArray().CopyTo(identityErrors, 0);
        return IdentityResult.Failed(identityErrors);
    }
}
