using Huybrechts.Core.Application;
using Huybrechts.Infra.Data;
using Microsoft.Extensions.Logging;

namespace Huybrechts.Infra.Application;

public class ChangeTenantStatusJob
{
    private readonly ApplicationContext _dbcontext;
    private readonly ILogger<ChangeTenantStatusJob> _logger;

    public ChangeTenantStatusJob(ApplicationContext dbcontext, ILogger<ChangeTenantStatusJob> logger)
    {
        _dbcontext = dbcontext;
        _logger = logger;
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

        // REMOVAL LOGIC FOR TENANTS
        // NONE SO FAR !

        item.State = ApplicationTenantState.Removed;
        _dbcontext.ApplicationTenants.Update(item);
        await _dbcontext.SaveChangesAsync();
    }
}
