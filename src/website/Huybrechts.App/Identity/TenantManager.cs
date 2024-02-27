using Huybrechts.App.Identity.Entities;
using Huybrechts.App.Data;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Identity;

namespace Huybrechts.App.Identity;

public interface ITenantManager
{

}

public class TenantManager : ITenantManager
{
    private readonly AuthenticationStateProvider _authenticationState;
    private readonly ApplicationUserManager _userManager;
    private readonly ApplicationRoleManager _roleManager;
    private readonly DatabaseContext _dbcontext;
    private readonly ILogger<TenantManager> _logger;

    public TenantManager(
        AuthenticationStateProvider authenticationState,
        ApplicationUserManager userManager, 
        ApplicationRoleManager roleManager,
        DatabaseContext dbcontext,
        ILogger<TenantManager> logger)
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

        }

        return await _userManager.GetApplicationTenantsAsync(user);
    }

    public async Task AddTenantAsync(ApplicationTenant tenant)
    {
        var state = await _authenticationState.GetAuthenticationStateAsync();
        var user = await _userManager.GetUserAsync(state.User) ??
            throw new ApplicationException("User not found while trying to create tenant");

        _dbcontext.ApplicationTenants.Add(tenant);
        await _userManager.AddToTenantAsync(user, tenant.Id, ApplicationRole.GetRoleName(ApplicationRoleValues.Owner));
        await _dbcontext.SaveChangesAsync();
    }

    public async Task UpdateTenantAsync(ApplicationTenant tenant)
    {
        var state = await _authenticationState.GetAuthenticationStateAsync();
        var user = await _userManager.GetUserAsync(state.User) ??
            throw new ApplicationException($"User '{state.User}' not found while trying to update tenant");

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

        var item = await _dbcontext.ApplicationTenants.FindAsync(tenant.Id) ??
            throw new ApplicationException($"Tenant '{tenant.Id}' not found while trying to delete tenant");
        _dbcontext.ApplicationTenants.Remove(tenant);
        await _dbcontext.SaveChangesAsync();
    }
}
