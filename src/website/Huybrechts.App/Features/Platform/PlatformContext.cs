using Finbuckle.MultiTenant.Abstractions;
using Huybrechts.App.Data;
using Huybrechts.Core.Platform;
using Huybrechts.Core.Setup;
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

        // Ignore SetupUnit in PlatformContext to prevent it from creating a table
        modelBuilder.Ignore<SetupUnit>();

        // call the base library implementation AFTER the above
        base.OnModelCreating(modelBuilder);

        // Ignore SetupUnit in PlatformContext to prevent it from creating a table
        modelBuilder.Ignore<SetupUnit>();

        // Configuration for PlatformRateUnit
        modelBuilder.Entity<PlatformRateUnit>(entity =>
        {
            entity.HasKey(e => e.Id);

            // Configure the foreign key to SetupUnit
            entity.HasOne(e => e.SetupUnit)
                  .WithMany() // No navigation property on SetupUnit
                  .HasForeignKey(e => e.SetupUnitId)
                  .HasConstraintName("FK_PlatformRateUnit_SetupUnit");
        });
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

    public DbSet<PlatformRateUnit> Units { get; set; }

    //
    // TODO
    //
    /*
    public DbSet<PlatformLocation> Locations { get; set; }

    public DbSet<PlatformMeasureUnit> MeasureUnits { get; set; }

    public DbSet<PlatformMeasureDefault> MeasureDefaults { get; set; }

    */
}
