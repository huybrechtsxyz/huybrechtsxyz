using Huybrechts.Infra.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Huybrechts.Infra.Data;

public class ApplicationUserManager : UserManager<ApplicationUser>
{
    public ApplicationUserManager(
        IUserStore<ApplicationUser> store,
        IOptions<IdentityOptions> optionsAccessor,
        IPasswordHasher<ApplicationUser> passwordHasher,
        IEnumerable<IUserValidator<ApplicationUser>> userValidators,
        IEnumerable<IPasswordValidator<ApplicationUser>> passwordValidators,
        ILookupNormalizer keyNormalizer,
        IdentityErrorDescriber errors,
        IServiceProvider services,
        ILogger<UserManager<ApplicationUser>> logger) 
        : base(store, optionsAccessor, passwordHasher, userValidators, passwordValidators, keyNormalizer, errors, services, logger)
    {
    }

    public override Task<IdentityResult> AddToRoleAsync(ApplicationUser user, string role)
    {
        return base.AddToRoleAsync(user, role);
    }

    public override Task<IdentityResult> AddToRolesAsync(ApplicationUser user, IEnumerable<string> roles)
    {
        return base.AddToRolesAsync(user, roles);
    }

    public override Task<IList<string>> GetRolesAsync(ApplicationUser user)
    {
        return base.GetRolesAsync(user);
    }

    public override Task<IList<ApplicationUser>> GetUsersInRoleAsync(string roleName)
    {
        return base.GetUsersInRoleAsync(roleName);
    }

    public override Task<bool> IsInRoleAsync(ApplicationUser user, string role)
    {
        return base.IsInRoleAsync(user, role);
    }

    public override Task<IdentityResult> RemoveFromRoleAsync(ApplicationUser user, string role)
    {
        return base.RemoveFromRoleAsync(user, role);
    }

    public override Task<IdentityResult> RemoveFromRolesAsync(ApplicationUser user, IEnumerable<string> roles)
    {
        return base.RemoveFromRolesAsync(user, roles);
    }
}
