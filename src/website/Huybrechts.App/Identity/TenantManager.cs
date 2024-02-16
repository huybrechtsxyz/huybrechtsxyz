using Huybrechts.App.Identity.Entities;
using Huybrechts.App.Data;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Logging;

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

    public async Task<IList<ApplicationTenant>> GetTenants()
    {
        if (_userManager is null)
            return [];

        var state = await _authenticationState.GetAuthenticationStateAsync();
        var user = await _userManager.GetUserAsync(state.User);
        if (user is null)
            return [];

        return await _userManager.GetApplicationTenantsAsync(user);
    }
}
