using Huybrechts.App.Data;
using Huybrechts.App.Identity.Entities;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Runtime.CompilerServices;

namespace Huybrechts.App.Identity;

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

        }

        return await _userManager.GetApplicationTenantsAsync(user);
    }

    public async Task<ApplicationTenant?> GetTenantAsync(string tenantId)
    {
        var state = await _authenticationState.GetAuthenticationStateAsync();
        var user = await _userManager.GetUserAsync(state.User) ??
            throw new ApplicationException("User not found while trying to retrieve tenant");

        var item = _dbcontext.ApplicationUserTenants.FirstOrDefaultAsync(q => q.UserId == user.Id && q.TenantId == tenantId);
        if (item is null)
            return null;

        return await _dbcontext.ApplicationTenants.FindAsync(tenantId);
    }

    public ApplicationTenant NewTenant()
    {
        return new ApplicationTenant()
        {
            Id = string.Empty,
            State = ApplicationTenantState.New
        };
    }

    public async Task AddTenantAsync(ApplicationTenant tenant)
    {
        var state = await _authenticationState.GetAuthenticationStateAsync();
        var user = await _userManager.GetUserAsync(state.User) ??
            throw new ApplicationException("User not found while trying to create tenant");

        tenant.Id = tenant.Id.Trim().ToLowerInvariant();

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
