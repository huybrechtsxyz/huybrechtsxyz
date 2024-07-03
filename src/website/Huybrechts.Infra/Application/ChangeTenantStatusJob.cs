using Huybrechts.Core.Application;
using Huybrechts.Infra.Data;
using Microsoft.Extensions.Logging;

namespace Huybrechts.Infra.Application;

public class ChangeTenantStatusJob
{
    private readonly ApplicationContext _dbcontext;

    public ChangeTenantStatusJob(ApplicationContext dbcontext)
    {
        _dbcontext = dbcontext;
    }

    public async Task ActivateAsync(ApplicationTenant tenant)
    {
        var item = await _dbcontext.ApplicationTenants.FindAsync(tenant.Id) ??
            throw new ApplicationException($"Tenant '{tenant.Id}' not found while trying to activate tenant");
        if (item.State == ApplicationTenantState.Active)
            return;

        // ACTIVATION LOGIC FOR TENANTS
        // NONE SO FAR !

        item.State = ApplicationTenantState.Active;
        _dbcontext.ApplicationTenants.Update(item);
        await _dbcontext.SaveChangesAsync();
    }

    public async Task RemoveAsync(ApplicationTenant tenant)
    {
        var item = await _dbcontext.ApplicationTenants.FindAsync(tenant.Id) ??
            throw new ApplicationException($"Tenant '{tenant.Id}' not found while trying to deactivate tenant");
        if (item.State == ApplicationTenantState.Removed)
            return;

        var roles = _dbcontext.Roles.Where(q => q.TenantId == item.Id);
        _dbcontext.RemoveRange(roles);

        var userRoles = _dbcontext.UserRoles.Where(q => q.TenantId == item.Id);
        _dbcontext.RemoveRange(userRoles);

        item.State = ApplicationTenantState.Removed;
        _dbcontext.ApplicationTenants.Remove(item);
        await _dbcontext.SaveChangesAsync();
    }
}
