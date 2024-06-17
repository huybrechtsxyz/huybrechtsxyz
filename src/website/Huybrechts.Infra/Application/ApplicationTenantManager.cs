using Huybrechts.Core.Application;
using Huybrechts.Infra.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Huybrechts.Infra.Application;

public interface IApplicationTenantManager
{
}

public class ApplicationTenantManager : IApplicationTenantManager
{
    private readonly ApplicationUserManager _userManager;
    private readonly ApplicationRoleManager _roleManager;
    private readonly ApplicationContext _dbcontext;
    private readonly ILogger<ApplicationTenantManager> _logger;

    public ApplicationTenantManager(
        ApplicationUserManager userManager, 
        ApplicationRoleManager roleManager,
        ApplicationContext dbcontext,
        ILogger<ApplicationTenantManager> logger)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _dbcontext = dbcontext;
        _logger = logger;
    }

    public static ApplicationTenant NewTenant()
    {
        return new ApplicationTenant()
        {
            State = ApplicationTenantState.New
        };
    }

    public static string NormalizeIdentifier(string identifier) => identifier?.Trim().ToLowerInvariant();

    public async Task<ApplicationTenant> CreateTenantAsync(ApplicationUser user, ApplicationTenant tenant)
    {
        ArgumentNullException.ThrowIfNull(user);
        ArgumentException.ThrowIfNullOrWhiteSpace(user.Id);
        ArgumentException.ThrowIfNullOrWhiteSpace(user.UserName);
        ArgumentNullException.ThrowIfNull(tenant);
        ArgumentException.ThrowIfNullOrWhiteSpace(tenant.Id);
        ArgumentException.ThrowIfNullOrWhiteSpace(tenant.Name);

        tenant.Id = NormalizeIdentifier(tenant.Id);

        var exists = await _dbcontext.ApplicationTenants.Where(q => q.Id == tenant.Id).ToListAsync();
        if (exists.Count != 0)
        {
            throw new ArgumentException($"Tenant with Id {tenant.Id} already exists", "tenant");
        }

        _dbcontext.ApplicationTenants.Add(tenant);
        await _dbcontext.SaveChangesAsync();

        var roles = ApplicationRole.GetDefaultTenantRoles(tenant.Id);
        foreach (var role in roles)
            await _roleManager.CreateAsync(role);

        var roleId = ApplicationRole.GetTenantRole(tenant.Id, ApplicationRoleValues.Owner);
        await _userManager.AddToRoleAsync(user, roleId);
        return tenant;
    }

    public async Task DeleteTenantAsync(ApplicationUser user, ApplicationTenant tenant)
    {
        var item = await _dbcontext.ApplicationTenants.FindAsync(tenant.Id) ??
            throw new ApplicationException($"Tenant '{tenant.Id}' not found while trying to delete tenant");

        var hasRole = await _userManager.IsInRoleAsync(user, ApplicationRole.GetTenantRole(tenant.Id, ApplicationRoleValues.Owner));
        if (!hasRole)
            throw new ApplicationException($"User '{user.NormalizedUserName}' is not the owner of the tenant '{tenant.Id}'");

        item.State = ApplicationTenantState.Removing;
        _dbcontext.ApplicationTenants.Update(item);
        await _dbcontext.SaveChangesAsync();
    }

    public async Task<IList<ApplicationTenant>> GetTenantsAsync(ApplicationUser user)
    {
        if (_userManager is null) return [];
        if (user is null) return [];

        if (await _userManager.IsInRoleAsync(user, ApplicationRole.SystemAdministrator))
        {
            return await _dbcontext.ApplicationTenants.ToListAsync();
        }

        return await _userManager.GetTenantsListAsync(user);
    }

    public async Task<ApplicationTenant?> GetTenantAsync(ApplicationUser user, string tenantId)
    {
        var tenants = await _userManager.GetTenantsListAsync(user);
        if (tenants.FirstOrDefault(q => q.Id == tenantId) is null)
        {
            return null;
        }

        return await _dbcontext.ApplicationTenants.FindAsync(tenantId);
    }

    public async Task UpdateTenantAsync(ApplicationUser user, ApplicationTenant tenant)
    {
        var hasRole = await _userManager.IsInRoleAsync(user, ApplicationRole.GetTenantRole(tenant.Id, ApplicationRoleValues.Owner));
        if (!hasRole)
            throw new ApplicationException($"User '{user.NormalizedUserName}' is not the owner of the tenant");

        var item = await _dbcontext.ApplicationTenants.FindAsync(tenant.Id) ??
            throw new ApplicationException($"Tenant '{tenant.Id}' not found while trying to update tenant");

        item.UpdateFrom(tenant);
        _dbcontext.ApplicationTenants.Update(tenant);
        await _dbcontext.SaveChangesAsync();
    }
}
