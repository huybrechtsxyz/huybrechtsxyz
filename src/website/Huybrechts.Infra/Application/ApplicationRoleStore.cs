using Huybrechts.Core.Application;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Huybrechts.Infra.Application;

public class ApplicationRoleStore : RoleStore<ApplicationRole>
{
    public ApplicationRoleStore(DbContext context, IdentityErrorDescriber? describer = null) :
        base(context, describer)
    {
    }

    public IQueryable<ApplicationTenant> Tenants
    {
        get
        {
            return Tenants.AsQueryable<ApplicationTenant>();
        }
    }
}
