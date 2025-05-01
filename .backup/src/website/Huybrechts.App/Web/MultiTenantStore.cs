using Finbuckle.MultiTenant;
using Finbuckle.MultiTenant.Abstractions;
using FluentResults;
using Huybrechts.App.Application;
using Huybrechts.App.Data;
using Huybrechts.Core.Application;

namespace Huybrechts.App.Web;

public class MultiTenantStore : IMultiTenantStore<TenantInfo>
{
    private readonly ApplicationContext _dbcontext;
    private readonly ApplicationTenantManager _tenantManager;

    public MultiTenantStore(ApplicationContext context, ApplicationTenantManager tenantManager)
    {
        _dbcontext = context;
        _tenantManager = tenantManager;
    }

    public async Task<IEnumerable<TenantInfo>> GetAllAsync()
    {
        var activeTenants = await _tenantManager.GetAllActiveTenantsAsync();
        if (activeTenants is null)
            return [];

        List<TenantInfo> list = [];
        foreach (var tenant in activeTenants)
        {
            list.Add(new TenantInfo { Identifier = tenant.Id, Id = tenant.Id, Name = tenant.Name });
        }
        return list;
    }

    public async Task<bool> TryAddAsync(TenantInfo tenantInfo)
    {
        ArgumentNullException.ThrowIfNull(tenantInfo);
        ArgumentException.ThrowIfNullOrEmpty(tenantInfo.Id);
        ArgumentException.ThrowIfNullOrEmpty(tenantInfo.Name);
        ApplicationTenant tenant = new()
        {
            Id = tenantInfo.Id,
            Name = tenantInfo.Name
        };
        Result result = await _tenantManager.TryCreateTenantAsync(tenant);
        if (result.IsSuccess)
            return true;
        return false;
    }

    public async Task<TenantInfo?> TryGetAsync(string id)
    {
        Result<ApplicationTenant> result = await _tenantManager.TryGetTenantAsync(id);
        if (result.IsSuccess)
        {
            return new TenantInfo()
            {
                Identifier = result.Value.Id,
                Id = result.Value.Id,
                Name = result.Value.Name
            };
        }
        return null;
    }

    public async Task<TenantInfo?> TryGetByIdentifierAsync(string identifier)
    {
        Result<ApplicationTenant> result = await _tenantManager.TryGetTenantAsync(identifier);
        if (result.IsSuccess)
        {
            return new TenantInfo()
            {
                Identifier = result.Value.Id,
                Id = result.Value.Id,
                Name = result.Value.Name
            };
        }
        return null;
    }

    public async Task<bool> TryRemoveAsync(string identifier)
    {
        Result result = await _tenantManager.TryRemoveAsync(identifier);
        if (result.IsSuccess)
            return true;
        return false;
    }

    public async Task<bool> TryUpdateAsync(TenantInfo tenantInfo)
    {
        ArgumentNullException.ThrowIfNull(tenantInfo);
        ArgumentException.ThrowIfNullOrEmpty(tenantInfo.Id);
        ArgumentException.ThrowIfNullOrEmpty(tenantInfo.Name);

        Result<ApplicationTenant> result = await _tenantManager.TryGetTenantAsync(tenantInfo.Id);
        if (!result.IsSuccess)
            return false;

        var tenant = result.Value;
        tenant.Name = tenantInfo.Name;
        result = await _tenantManager.TryUpdateTenantAsync(tenant);

        if (result.IsSuccess)
            return true;
        return false;
    }
}
