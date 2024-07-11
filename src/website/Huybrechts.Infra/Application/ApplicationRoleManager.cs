using Huybrechts.Core.Application;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Huybrechts.Infra.Application;

public class ApplicationRoleManager : RoleManager<ApplicationRole>
{
    private readonly ApplicationRoleStore _store;

    public ApplicationRoleManager(
        ApplicationRoleStore store, 
        IEnumerable<IRoleValidator<ApplicationRole>> roleValidators, 
        ILookupNormalizer keyNormalizer, IdentityErrorDescriber errors, 
        ILogger<RoleManager<ApplicationRole>> logger) 
        : base(store, roleValidators, keyNormalizer, errors, logger)
    {
        _store = store;
    }
}
