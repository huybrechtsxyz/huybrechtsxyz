using Huybrechts.Core.Application;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Huybrechts.App.Application;

public class ApplicationRoleManager : RoleManager<ApplicationRole>
{
    public ApplicationRoleManager(
        ApplicationRoleStore store, 
        IEnumerable<IRoleValidator<ApplicationRole>> roleValidators, 
        ILookupNormalizer keyNormalizer, IdentityErrorDescriber errors, 
        ILogger<RoleManager<ApplicationRole>> logger) 
        : base(store, roleValidators, keyNormalizer, errors, logger)
    {
    }
}
