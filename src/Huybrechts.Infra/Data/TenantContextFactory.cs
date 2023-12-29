using Huybrechts.Infra.Config;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Huybrechts.Infra.Data;

public class TenantContextFactory
{
    public TenantContextFactory()
    {
        
    }

    public static DbContextOptionsBuilder BuildOptions(string? connectionString, DatabaseProviderType connectionType)
    {
        ArgumentNullException.ThrowIfNullOrEmpty(connectionString);

        DbContextOptionsBuilder builder = new();
        switch (connectionType)
        {
            case DatabaseProviderType.SqlServer:
                {
                    builder.UseSqlServer(connectionString);
                    break;
                }
            case DatabaseProviderType.PostgreSQL:
                {
                    builder.UseNpgsql(connectionString);
                    break;
                }
            case DatabaseProviderType.None:
                break;
            default:
                {
                    builder.UseSqlite(connectionString);
                    break;
                }
        }
        return builder;
    }

    public ITenantContextCollection BuildTenantCollection(IConfiguration configuration)
    {
        ApplicationSettings applicationSettings = new(configuration);
        DatabaseProviderType connectionType = applicationSettings.GetAdministrationDatabaseType();
        var connectionString = applicationSettings.GetAdministrationConnectionString();
        var contextOptions = TenantContextFactory.BuildOptions(connectionString, connectionType);
        AdministrationContext dbcontext = new(contextOptions.Options);
        var collection = BuildTenantCollection(dbcontext);
        dbcontext.Dispose();
        return collection;
    }

    public ITenantContextCollection BuildTenantCollection(AdministrationContext dbcontext)
    {
        var tenants = dbcontext.Tenants.ToList();
        if (tenants is null || tenants.Count == 0)
            return new TenantContextCollection();

        TenantContextCollection collection = new();
        foreach (var item in tenants)
        {
            collection.SetTenant(item.Code, BuildContext(item));
        }
        return collection;
    }

    public TenantContext BuildContext(ApplicationTenant tenant)
    {
        ApplicationTenant value = new()
        {
            Code = tenant.Code,
            ConnectionString = tenant.ConnectionString,
            DatabaseProvider = tenant.DatabaseProvider
        };
        var options = BuildOptions(tenant.ConnectionString, tenant.GetDatabaseProviderType());
        return new(options.Options, value);
    }
}

public interface ITenantContextCollection : IDisposable
{
    TenantContext GetTenant(string tenant);

    void SetTenant(string tenant, TenantContext value);
}

public class TenantContextCollection: ITenantContextCollection
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

public class TenantContext: DbContext
{
    public ApplicationTenant Tenant { get; init; }

    public TenantContext(DbContextOptions options, ApplicationTenant tenant)
        : base(options)
    {
        Tenant = tenant;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
    }
}