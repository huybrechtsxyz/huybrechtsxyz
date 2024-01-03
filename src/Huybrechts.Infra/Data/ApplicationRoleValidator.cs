using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Huybrechts.Infra.Data;

public class ApplicationRoleValidator : IRoleValidator<ApplicationRole>
{
    public async Task<IdentityResult> ValidateAsync(RoleManager<ApplicationRole> manager, ApplicationRole role)
    {
        ArgumentNullException.ThrowIfNull(manager, nameof(manager));
        ArgumentNullException.ThrowIfNull(role, nameof(role));

        var errors = new List<string>();

        if (!string.IsNullOrWhiteSpace(role.Name))
        {
            var existingRole = await manager.Roles.FirstOrDefaultAsync(x => x.TenantId == role.TenantId && x.Name == role.Name);
            if (existingRole is null)
                return IdentityResult.Success;

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
