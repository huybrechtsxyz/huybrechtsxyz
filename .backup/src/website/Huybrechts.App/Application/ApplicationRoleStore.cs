using Huybrechts.App.Data;
using Huybrechts.Core.Application;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace Huybrechts.App.Application;

public class ApplicationRoleStore : RoleStore<ApplicationRole,ApplicationContext,string,ApplicationUserRole,ApplicationRoleClaim>
{
    public ApplicationRoleStore(ApplicationContext context, IdentityErrorDescriber? describer = null) :
        base(context, describer)
    {
    }
}
