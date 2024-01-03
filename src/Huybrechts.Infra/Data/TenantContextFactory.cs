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

    public static DbContextOptionsBuilder BuildOptions<Tcontext>(string? connectionString, DatabaseProviderType connectionType) where Tcontext : DbContext
    {
        ArgumentNullException.ThrowIfNullOrEmpty(connectionString);

        DbContextOptionsBuilder<Tcontext> builder = new();
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

    public void BuildTenantCollection(AdministrationContext dbcontext, TenantContextCollection collection)
    {
        var tenants = dbcontext.Tenants.ToList();
        if (tenants is not null && tenants.Count != 0)
        {
            foreach (var item in tenants)
            {
                if (collection.TryGetTenant(item.Id, out _))
                    continue;
                collection.SetTenant(item.Id, BuildContext(item));
            }
        }
    }

    public TenantContext BuildContext(ApplicationTenant tenant)
    {
        ApplicationTenant value = new()
        {
            Id = tenant.Id,
            ConnectionString = tenant.ConnectionString,
            DatabaseProvider = tenant.DatabaseProvider
        };
        var options = BuildOptions<TenantContext>(tenant.ConnectionString, tenant.GetDatabaseProviderType());
        return new(options.Options, value);
    }
}
