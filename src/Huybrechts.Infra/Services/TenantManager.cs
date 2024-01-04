using Huybrechts.Infra.Data;
using Huybrechts.Infra.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Org.BouncyCastle.Crypto.Digests;

namespace Huybrechts.Infra.Services;

public interface ITenantManager
{

}

public class TenantManager : ITenantManager
{
    private readonly HttpContextAccessor _httpContextAccessor;
    private readonly ApplicationUserManager _userManager;
    private readonly ApplicationRoleManager _roleManager;
    private readonly AdministrationContext _dbcontext;
    private readonly ILogger _logger;

    public TenantManager(
        HttpContextAccessor httpContextAccessor,
        ApplicationUserManager userManager, 
        ApplicationRoleManager roleManager,
        AdministrationContext dbcontext,
        ILogger logger)
    {
        _httpContextAccessor = httpContextAccessor;
        _userManager = userManager;
        _roleManager = roleManager;
        _dbcontext = dbcontext;
        _logger = logger;
    }

    public async List<ApplicationTenant> GetTenants()
    {
        if (_httpContextAccessor is null || _httpContextAccessor.HttpContext is null || _userManager is null)
            return [];

        var user = await _userManager.GetUserAsync(_httpContextAccessor.HttpContext.User);
        var applicationUsers = _userManager.Users.Include(i => i.Tenants).Where(q => q.Id == user.Id).ToList();
        
        return new List<ApplicationTenant> { new() { Id = "jef" } };
    }
}
