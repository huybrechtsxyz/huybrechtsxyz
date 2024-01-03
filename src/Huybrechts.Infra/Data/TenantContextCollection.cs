using System.Collections;

namespace Huybrechts.Infra.Data;

public class TenantContextCollection : ITenantContextCollection, IEnumerable
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

    public bool TryGetTenant(string tenant, out TenantContext? value) => _tenants.TryGetValue(tenant, out value);

    public void SetTenant(string tenant, TenantContext value)
    {
        ArgumentNullException.ThrowIfNull(value);
        if (!_tenants.TryAdd(tenant, value))
            throw new ArgumentException("Tenant with key already exists", nameof(tenant));
    }

    public IEnumerator GetEnumerator()
    {
        return _tenants.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
