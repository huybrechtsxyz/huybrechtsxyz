using Huybrechts.Infra.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Huybrechts.Infra.Services;

public interface ITenantManager
{

}

public class TenantManager : ITenantManager
{
    private HttpContext _httpContext;
    private ApplicationTenant _currentTenant;

    public TenantManager(IHttpContextAccessor contextAccessor)
    {
       
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
