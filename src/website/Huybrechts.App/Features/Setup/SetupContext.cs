using Finbuckle.MultiTenant.Abstractions;
using Huybrechts.App.Data;
using Huybrechts.Core.Platform;
using Huybrechts.Core.Setup;
using Microsoft.EntityFrameworkCore;

namespace Huybrechts.App.Features.Setup;

public class SetupContext : FeatureContext
{
    public SetupContext(IMultiTenantContextAccessor multiTenantContextAccessor, DbContextOptions options) 
        : base(multiTenantContextAccessor, options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // set a global query filter, e.g. to support soft delete
        //modelBuilder.Entity<Post>().HasQueryFilter(p => !p.IsDeleted);

        if (Database.IsSqlite()) 
        { 
            base.SetTimeStampForFieldsForSqlite(modelBuilder);
        }

        // call the base library implementation AFTER the above
        base.OnModelCreating(modelBuilder);
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder
            .Properties<Ulid>()
            .HaveConversion<UlidToStringConverter>();
    }

    public DbSet<SetupUnit> SystemUnits { get; set; }
}
