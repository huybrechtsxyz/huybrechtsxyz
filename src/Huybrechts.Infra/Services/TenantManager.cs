using Huybrechts.Infra.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Huybrechts.Infra.Services;

public interface ITenantManager
{
    public string GetDatabaseProvider();

    public string GetConnectionString();

    public ApplicationTenant GetTenant();
}

public class TenantManager : ITenantManager
{
    private HttpContext _httpContext;
    private ApplicationTenant _currentTenant;

    public TenantManager(IHttpContextAccessor contextAccessor)
    {
        _httpContext = contextAccessor.HttpContext!;
        if (_httpContext is not null)
        {
            if (_httpContext.Request.Headers.TryGetValue("tenant", out var tenantId))
            {
                SetTenant(tenantId);
            }
            else
            {
                throw new Exception("Invalid Tenant!");
            }
        }
    }

    public string GetConnectionString()
    {
        throw new NotImplementedException();
    }

    public string GetDatabaseProvider()
    {
        throw new NotImplementedException();
    }

    public ApplicationTenant GetTenant()
    {
        return _currentTenant;
    }
}
