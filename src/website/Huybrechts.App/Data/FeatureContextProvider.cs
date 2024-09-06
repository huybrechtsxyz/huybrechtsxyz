using Finbuckle.MultiTenant.Abstractions;
using Finbuckle.MultiTenant;

namespace Huybrechts.App.Data;

public interface IFeatureContextProvider
{
    FeatureContext GetMultiTenantFeatureContext();

    FeatureContext GetMultiTenantFeatureContext(TenantInfo tenantInfo);

    Task<FeatureContext?> GetMultiTenantFeatureContextAsync(string tenantId);
}

public class FeatureContextProvider : IFeatureContextProvider
{
    private readonly IMultiTenantContextAccessor _multiTenantContextAccessor;
    private readonly IMultiTenantContextSetter _multiTenantContextSetter;
    private readonly IMultiTenantStore<TenantInfo> _multiTenantStore;
    private readonly FeatureContext _featureContext;

    public FeatureContextProvider(
        IMultiTenantContextAccessor multiTenantContextAccessor,
        IMultiTenantContextSetter multiTenantContextSetter,
        IMultiTenantStore<TenantInfo> multiTenantStore,
        FeatureContext featureContext)
    {
        _multiTenantContextAccessor = multiTenantContextAccessor;
        _multiTenantContextSetter = multiTenantContextSetter;
        _multiTenantStore = multiTenantStore;
        _featureContext = featureContext;
    }

    public FeatureContext GetMultiTenantFeatureContext()
    {
        var tenant = _multiTenantContextAccessor.MultiTenantContext.TenantInfo;
        _multiTenantContextSetter.MultiTenantContext = new MultiTenantContext<TenantInfo>()
        {
            TenantInfo = (TenantInfo)tenant!
        };

        return _featureContext;
    }

    public FeatureContext GetMultiTenantFeatureContext(TenantInfo tenantInfo)
    {
        _multiTenantContextSetter.MultiTenantContext = new MultiTenantContext<TenantInfo>()
        {
            TenantInfo = tenantInfo
        };

        return _featureContext;
    }

    public async Task<FeatureContext?> GetMultiTenantFeatureContextAsync(string tenantId)
    {
        var tenantInfo = await _multiTenantStore.TryGetByIdentifierAsync(tenantId);
        if (tenantInfo is null) return null;

        _multiTenantContextSetter.MultiTenantContext = new MultiTenantContext<TenantInfo>()
        {
            TenantInfo = tenantInfo
        };

        return _featureContext;
    }
}