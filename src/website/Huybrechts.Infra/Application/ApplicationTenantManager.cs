using Hangfire;
using Huybrechts.Core.Application;
using Huybrechts.Infra.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Threading;

namespace Huybrechts.Infra.Application;

public interface IApplicationTenantManager
{
}

public class ApplicationTenantManager : IApplicationTenantManager
{
    public static ApplicationTenant NewTenant()
    {
        return new ApplicationTenant()
        {
            State = ApplicationTenantState.New
        };
    }

    public static string NormalizeIdentifier(string identifier) => (identifier ?? string.Empty).Trim().ToLowerInvariant();

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

    public async Task ActivateTenantAsync(ApplicationUser user, ApplicationTenant tenant)
    {
        if (!await _userManager.IsOwnerAsync(user, tenant.Id))
            throw new ApplicationException($"User '{user.NormalizedUserName}' is not the owner of the tenant");

        var item = await _dbcontext.ApplicationTenants.FindAsync(tenant.Id) ??
            throw new ApplicationException($"Tenant '{tenant.Id}' not found while trying to update tenant state");

        if (item.State != ApplicationTenantState.New && item.State != ApplicationTenantState.Disabled)
            throw new ApplicationException($"Tenant '{tenant.Id}' is not in state new or inactive");

        item.State = ApplicationTenantState.Pending;
        _dbcontext.ApplicationTenants.Update(item);
        await _dbcontext.SaveChangesAsync();

        BackgroundJob.Enqueue<ChangeTenantStatusJob>(x => x.ActivateAsync(item));
    }

    public async Task<ApplicationTenant> CreateTenantAsync(ApplicationUser user, ApplicationTenant tenant)
    {
        ArgumentNullException.ThrowIfNull(user);
        ArgumentException.ThrowIfNullOrWhiteSpace(user.Id);
        ArgumentException.ThrowIfNullOrWhiteSpace(user.UserName);
        ArgumentNullException.ThrowIfNull(tenant);
        ArgumentException.ThrowIfNullOrWhiteSpace(tenant.Id);
        ArgumentException.ThrowIfNullOrWhiteSpace(tenant.Name);

        ApplicationTenant item = new()
        {
            Id = NormalizeIdentifier(tenant.Id),
            Name = tenant.Name,
            Description = tenant.Description,
            Remark = tenant.Remark,
            State = ApplicationTenantState.New,
            ConnectionString = null,
            DatabaseProvider = null
        };
        if (tenant.Picture is not null)
        {
            item.Picture ??= new byte[tenant.Picture.Length];
            Array.Copy(tenant.Picture, item.Picture, tenant.Picture.Length);
        }
        else
            item.Picture = null;

        var exists = await _dbcontext.ApplicationTenants.Where(q => q.Id == item.Id).ToListAsync();
        if (exists.Count != 0)
            throw new ArgumentException($"Tenant with Id {tenant.Id} already exists", nameof(tenant));

        _dbcontext.ApplicationTenants.Add(item);
        await _dbcontext.SaveChangesAsync();

        var roles = ApplicationRole.GetDefaultTenantRoles(item.Id);
        foreach (var role in roles)
            await _roleManager.CreateAsync(role);

        var roleId = ApplicationRole.GetRoleName(item.Id, ApplicationDefaultTenantRole.Owner);
        await _userManager.AddToRoleAsync(user, roleId);
        return item;
    }

    public async Task DeleteTenantAsync(ApplicationUser user, ApplicationTenant tenant)
    {
        var item = await _dbcontext.ApplicationTenants.FindAsync(tenant.Id) ??
            throw new ApplicationException($"Tenant '{tenant.Id}' not found while trying to delete tenant");

        if (!await _userManager.IsOwnerAsync(user, tenant.Id))
            throw new ApplicationException($"User '{user.NormalizedUserName}' is not the owner of the tenant '{tenant.Id}'");

        item.State = ApplicationTenantState.Removing;
        _dbcontext.ApplicationTenants.Update(item);
        await _dbcontext.SaveChangesAsync();

        BackgroundJob.Enqueue<ChangeTenantStatusJob>(x => x.RemoveAsync(item));
    }

    public async Task DisableTenantAsync(ApplicationUser user, ApplicationTenant tenant)
    {
        if (!await _userManager.IsOwnerAsync(user, tenant.Id))
            throw new ApplicationException($"User '{user.NormalizedUserName}' is not the owner of the tenant");

        var item = await _dbcontext.ApplicationTenants.FindAsync(tenant.Id) ??
            throw new ApplicationException($"Tenant '{tenant.Id}' not found while trying to update tenant state");

        if (item.State != ApplicationTenantState.Active)
            throw new ApplicationException($"Tenant '{tenant.Id}' is not in state active");

        item.State = ApplicationTenantState.Disabled;
        _dbcontext.ApplicationTenants.Update(item);
        await _dbcontext.SaveChangesAsync();
    }

    public async Task<IList<ApplicationTenant>> GetTenantsAsync(ApplicationUser user, CancellationToken cancellationToken = default)
    {
        if (user is null) return [];
        if (string.IsNullOrEmpty(user.Id)) return [];
        
        if (await _userManager.IsAdministratorAsync(user))
        {
            return await _dbcontext.ApplicationTenants.ToListAsync();
        }

        var userId = user.Id;
        var query = from userRole in _dbcontext.UserRoles
                    join tenant in _dbcontext.ApplicationTenants on userRole.TenantId equals tenant.Id
                    where userRole.UserId.Equals(userId) && tenant.State != ApplicationTenantState.Removed
                    select tenant;
        
        return await query.GroupBy(q => q.Name).Select(q => q.First()).ToListAsync(cancellationToken);
    }

    public async Task<ApplicationTenant?> GetTenantAsync(ApplicationUser user, string tenantId)
    {
        var tenant = await _dbcontext.ApplicationTenants.FirstOrDefaultAsync(q => q.Id == tenantId);
        if (tenant == null)
            return null;

        if (await _userManager.IsInTenantAsync(user.Id, tenantId))
            return tenant;
        
        if (await _userManager.IsAdministratorAsync(user))
            return tenant;

        return null;
    }

    public async Task<List<ApplicationRole>> GetTenantRolesAsync(ApplicationUser user, string tenantId)
    {
        return await _dbcontext.Roles.Where(q => q.TenantId == tenantId).ToListAsync() ?? [];
    }

    public async Task<List<ApplicationUserRole>> GetTenantUserRolesAsync(ApplicationUser user, string tenantId)
    {
        return await _dbcontext.UserRoles
            .Include(i => i.Role)
            .Include(i => i.User)
            .Where(q => q.TenantId == tenantId).ToListAsync() ?? [];
    }

    public async Task UpdateTenantAsync(ApplicationUser user, ApplicationTenant tenant)
    {
        if (!await _userManager.IsOwnerAsync(user, tenant.Id))
            throw new ApplicationException($"User '{user.NormalizedUserName}' is not the owner of the tenant");

        var item = await _dbcontext.ApplicationTenants.FindAsync(tenant.Id) ??
            throw new ApplicationException($"Tenant '{tenant.Id}' not found while trying to update tenant");

        item.UpdateFrom(tenant);
        _dbcontext.ApplicationTenants.Update(item);
        await _dbcontext.SaveChangesAsync();
    }
}
