using Finbuckle.MultiTenant.Abstractions;
using Finbuckle.MultiTenant.EntityFrameworkCore;
using Huybrechts.Core.Platform;
using Huybrechts.Core.Setup;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Reflection;

namespace Huybrechts.App.Data;

public class FeatureContext : MultiTenantDbContext, IMultiTenantDbContext
{
    private IDbContextTransaction? _currentTransaction;

    public FeatureContext(IMultiTenantContextAccessor multiTenantContextAccessor, DbContextOptions options) : base(multiTenantContextAccessor, options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // set a global query filter, e.g. to support soft delete
        //modelBuilder.Entity<Post>().HasQueryFilter(p => !p.IsDeleted);
        if (Database.IsSqlite())
        {
            SetTimeStampForFieldsForSqlite(modelBuilder);
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

    protected void SetTimeStampForFieldsForSqlite(ModelBuilder modelBuilder)
    {
        if (!Database.IsSqlite())
            return;

        var entityTypes = modelBuilder.Model.GetEntityTypes();
        foreach (var entityType in entityTypes)
        {
            var clrType = entityType.ClrType;
            var timestampProperties = clrType.GetProperties()
                .Where(p => p.GetCustomAttribute<TimestampAttribute>() != null);
            foreach (var property in timestampProperties)
            {
                modelBuilder.Entity(clrType)
                    .Property(property.Name)
                    .ValueGeneratedOnAddOrUpdate()
                    .IsConcurrencyToken()
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");
            }
        }
    }

    public async Task BeginTransactionAsync()
    {
        if (_currentTransaction != null)
        {
            return;
        }

        _currentTransaction = await Database.BeginTransactionAsync(IsolationLevel.ReadCommitted);
    }

    public async Task CommitTransactionAsync()
    {
        try
        {
            await SaveChangesAsync();

            await (_currentTransaction?.CommitAsync() ?? Task.CompletedTask);
        }
        catch
        {
            RollbackTransaction();
            throw;
        }
        finally
        {
            if (_currentTransaction != null)
            {
                _currentTransaction.Dispose();
                _currentTransaction = null!;
            }
        }
    }

    public void RollbackTransaction()
    {
        try
        {
            _currentTransaction?.Rollback();
        }
        finally
        {
            if (_currentTransaction != null)
            {
                _currentTransaction.Dispose();
                _currentTransaction = null!;
            }
        }
    }

    //
    // SETUP
    //

    public DbSet<SetupCountry> SetupCountries { get; set; }

    public DbSet<SetupCurrency> SetupCurrencies { get; set; }

    public DbSet<SetupLanguage> SetupLanguages { get; set; }

    public DbSet<SetupUnit> SetupUnits { get; set; }

    //
    // PLATFORM
    //

    public DbSet<PlatformInfo> Platforms { get; set; }

    public DbSet<PlatformRegion> PlatformRegions { get; set; }

    public DbSet<PlatformService> PlatformServices { get; set; }

    public DbSet<PlatformProduct> PlatformProducts { get; set; }

    public DbSet<PlatformRate> PlatformRates { get; set; }

    public DbSet<PlatformRateUnit> PlatformRateUnits { get; set; }
}
