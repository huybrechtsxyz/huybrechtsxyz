namespace Huybrechts.Infra.Data;

public interface ITenantContextCollection : IDisposable
{
    TenantContext GetTenant(string tenant);

    void SetTenant(string tenant, TenantContext value);
}
