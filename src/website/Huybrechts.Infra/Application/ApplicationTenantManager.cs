using Huybrechts.Core.Application;
using Huybrechts.Infra.Data;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Huybrechts.Infra.Application;

public interface IApplicationTenantManager
{

}

public class ApplicationTenantManager : IApplicationTenantManager
{
    private readonly AuthenticationStateProvider _authenticationState;
    private readonly ApplicationUserManager _userManager;
    private readonly ApplicationRoleManager _roleManager;
    private readonly ApplicationContext _dbcontext;
    private readonly ILogger<ApplicationTenantManager> _logger;

    public ApplicationTenantManager(
        AuthenticationStateProvider authenticationState,
        ApplicationUserManager userManager, 
        ApplicationRoleManager roleManager,
        ApplicationContext dbcontext,
        ILogger<ApplicationTenantManager> logger)
    {
        _authenticationState = authenticationState;
        _userManager = userManager;
        _roleManager = roleManager;
        _dbcontext = dbcontext;
        _logger = logger;
    }

    public async Task<IList<ApplicationTenant>> GetTenantsAsync()
    {
        if (_userManager is null)
            return [];

        var state = await _authenticationState.GetAuthenticationStateAsync();
        var user = await _userManager.GetUserAsync(state.User);
        if (user is null)
            return [];
        if (await _userManager.IsInRoleAsync(user, ApplicationRole.SystemAdministrator))
        {
            return await _dbcontext.ApplicationTenants.ToListAsync();
        }

        return await _userManager.GetTenantsListAsync(user);
    }

    public async Task<ApplicationTenant?> GetTenantAsync(string tenantId)
    {
        var state = await _authenticationState.GetAuthenticationStateAsync();
        var user = await _userManager.GetUserAsync(state.User) ??
            throw new ApplicationException("User not found while trying to retrieve tenant");

        return await _dbcontext.ApplicationTenants.FindAsync(tenantId);        
    }

    public static ApplicationTenant NewTenant()
    {
        return new ApplicationTenant()
        {
            Id = string.Empty,
            State = ApplicationTenantState.New
        };
    }

    public async Task<ApplicationTenant> AddTenantAsync(ApplicationTenant tenant)
    {
        var state = await _authenticationState.GetAuthenticationStateAsync();
        var user = await _userManager.GetUserAsync(state.User) ??
            throw new ApplicationException("User not found while trying to create tenant");

        tenant.Id = tenant.Id.Trim().ToLowerInvariant();
        var roleId = ApplicationRole.GetTenantRole(tenant.Id, ApplicationRoleValues.Owner);

        _dbcontext.ApplicationTenants.Add(tenant);
        await _userManager.AddToRoleAsync(user, roleId);
        await _dbcontext.SaveChangesAsync();
        return tenant;
    }

    public async Task UpdateTenantAsync(ApplicationTenant tenant)
    {
        var state = await _authenticationState.GetAuthenticationStateAsync();
        var user = await _userManager.GetUserAsync(state.User) ??
            throw new ApplicationException($"User '{state.User}' not found while trying to update tenant");
        
        var hasRole = await _userManager.IsInRoleAsync(user, ApplicationRole.GetTenantRole(tenant.Id, ApplicationRoleValues.Owner));
        if (!hasRole)
            throw new ApplicationException($"User '{user.NormalizedUserName}' is not the owner of the tenant");

        var item = await _dbcontext.ApplicationTenants.FindAsync(tenant.Id) ??
            throw new ApplicationException($"Tenant '{tenant.Id}' not found while trying to update tenant");

        item.UpdateFrom(tenant);
        _dbcontext.ApplicationTenants.Update(tenant);
        await _dbcontext.SaveChangesAsync();
    }

    public async Task DeleteTenantAsync(ApplicationTenant tenant)
    {
        var state = await _authenticationState.GetAuthenticationStateAsync();
        var user = await _userManager.GetUserAsync(state.User) ??
            throw new ApplicationException($"User '{state.User}' not found while trying to delete tenant");

        var hasRole = await _userManager.IsInRoleAsync(user, ApplicationRole.GetTenantRole(tenant.Id, ApplicationRoleValues.Owner));
        if (!hasRole)
            throw new ApplicationException($"User '{user.NormalizedUserName}' is not the owner of the tenant");

        var item = await _dbcontext.ApplicationTenants.FindAsync(tenant.Id) ??
            throw new ApplicationException($"Tenant '{tenant.Id}' not found while trying to delete tenant");

        item.State = ApplicationTenantState.Removing;
        _dbcontext.ApplicationTenants.Update(item);
        await _dbcontext.SaveChangesAsync();
    }
}
