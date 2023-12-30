namespace Huybrechts.Infra.Data;

public class TenantContextCollection : ITenantContextCollection
{
    private readonly Dictionary<string, TenantContext> _tenants = [];

    public TenantContextCollection() 
    {
    }

    public void Dispose()
    {
        if (_tenants is null)
            return;
        foreach(var tenant in _tenants)
            tenant.Value.Dispose();
    }

    public TenantContext GetTenant(string tenant)
    {
        if (_tenants.TryGetValue(tenant, out TenantContext? value))
            return value;
        throw new Exception("Invalid tenant requested");
    }

    public void SetTenant(string tenant, TenantContext value)
    {
        ArgumentNullException.ThrowIfNull(value);
        if (!_tenants.TryAdd(tenant, value))
            throw new ArgumentException("Tenant with key already exists", nameof(tenant));
    }
}
