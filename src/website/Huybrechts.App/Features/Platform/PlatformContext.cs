using Finbuckle.MultiTenant.Abstractions;
using Huybrechts.App.Data;
using Huybrechts.Core.Platform;
using Microsoft.EntityFrameworkCore;

namespace Huybrechts.App.Features.Platform;

public class PlatformContext : FeatureContext
{
    public PlatformContext(IMultiTenantContextAccessor multiTenantContextAccessor, DbContextOptions options) 
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

    public DbSet<PlatformInfo> Platforms { get; set; }

    public DbSet<PlatformRegion> Regions { get; set; }

    public DbSet<PlatformService> Services { get; set; }

    public DbSet<PlatformProduct> Products { get; set; }

    public DbSet<PlatformRate> Rates { get; set; }

    //
    // TODO
    //
    /*
    public DbSet<PlatformLocation> Locations { get; set; }

    public DbSet<PlatformMeasureUnit> MeasureUnits { get; set; }

    public DbSet<PlatformMeasureDefault> MeasureDefaults { get; set; }

    public DbSet<PlatformRate> Rates { get; set; }

    public DbSet<PlatformRateUnit> RateUnits { get; set; }

    public DbSet<PlatformSearchRate> SearchRates { get; set; }
    */
}
