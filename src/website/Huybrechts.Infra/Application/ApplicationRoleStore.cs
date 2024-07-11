using Huybrechts.Core.Application;
using Huybrechts.Infra.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace Huybrechts.Infra.Application;

public class ApplicationRoleStore : RoleStore<ApplicationRole,ApplicationContext,string,ApplicationUserRole,ApplicationRoleClaim>
{
    private readonly ApplicationContext _context;

    public ApplicationRoleStore(ApplicationContext context, IdentityErrorDescriber? describer = null) :
        base(context, describer)
    {
        _context = context;
    }
}
