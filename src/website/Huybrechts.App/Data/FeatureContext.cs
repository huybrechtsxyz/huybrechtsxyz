﻿using Finbuckle.MultiTenant.Abstractions;
using Finbuckle.MultiTenant.EntityFrameworkCore;
using Huybrechts.Core.Platform;
using Huybrechts.Core.Project;
using Huybrechts.Core.Setup;
using Huybrechts.Core.Wiki;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Reflection;

namespace Huybrechts.App.Data;

public class FeatureContext : MultiTenantDbContext, IMultiTenantDbContext
{
    private IDbContextTransaction? _currentTransaction;

    public FeatureContext(IMultiTenantContextAccessor multiTenantContextAccessor, DbContextOptions options) 
        : base(multiTenantContextAccessor, options)
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

        // Configure the relationship between ProjectComponentUnit and others
        #region ProjectComponentUnit
        modelBuilder.Entity<ProjectComponentUnit>().HasOne(p => p.ProjectQuantity).WithMany().HasForeignKey(p => p.ProjectQuantityId).OnDelete(DeleteBehavior.Restrict);
        #endregion ProjectComponentUnit

        // Configure the relationship between ProjectSimulationEntry and others
        #region ProjectSimulationEntry
        modelBuilder.Entity<ProjectSimulationEntry>().HasOne(p => p.ProjectInfo).WithMany().HasForeignKey(p => p.ProjectInfoId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<ProjectSimulationEntry>().HasOne(p => p.ProjectScenario).WithMany().HasForeignKey(p => p.ProjectScenarioId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<ProjectSimulationEntry>().HasOne(p => p.ProjectDesign).WithMany().HasForeignKey(p => p.ProjectDesignId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<ProjectSimulationEntry>().HasOne(p => p.ProjectComponent).WithMany().HasForeignKey(p => p.ProjectComponentId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<ProjectSimulationEntry>().HasOne(p => p.PlatformInfo).WithMany().HasForeignKey(p => p.PlatformInfoId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<ProjectSimulationEntry>().HasOne(p => p.PlatformProduct).WithMany().HasForeignKey(p => p.PlatformProductId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<ProjectSimulationEntry>().HasOne(p => p.PlatformRegion).WithMany().HasForeignKey(p => p.PlatformRegionId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<ProjectSimulationEntry>().HasOne(p => p.PlatformService).WithMany().HasForeignKey(p => p.PlatformServiceId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<ProjectSimulationEntry>().HasOne(p => p.PlatformRate).WithMany().HasForeignKey(p => p.PlatformRateId).OnDelete(DeleteBehavior.Restrict);
        #endregion ProjectSimulationEntry

        // Configure the search index for wiki
        if (Database.IsSqlServer()) {}
        else if (Database.IsNpgsql()) {}
        else if (Database.IsSqlite()) {}
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

    public async Task BeginTransactionAsync(CancellationToken token = default)
    {
        if (_currentTransaction != null)
        {
            return;
        }

        _currentTransaction = await Database.BeginTransactionAsync(IsolationLevel.ReadCommitted, token);
    }

    public async Task CommitTransactionAsync(CancellationToken token = default)
    {
        try
        {
            await SaveChangesAsync(token);

            await (_currentTransaction?.CommitAsync(token) ?? Task.CompletedTask);
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

    public DbSet<SetupState> SetupStates { get; set; }

    public DbSet<SetupUnit> SetupUnits { get; set; }

    //
    // PLATFORM
    //

    public DbSet<PlatformInfo> Platforms { get; set; }

    public DbSet<PlatformDefaultUnit> PlatformDefaultUnits { get; set; }

    public DbSet<PlatformRegion> PlatformRegions { get; set; }

    public DbSet<PlatformService> PlatformServices { get; set; }

    public DbSet<PlatformProduct> PlatformProducts { get; set; }

    public DbSet<PlatformRate> PlatformRates { get; set; }

    public DbSet<PlatformRateUnit> PlatformRateUnits { get; set; }

    //
    // PROJECT
    //

    public DbSet<ProjectInfo> Projects { get; set; }

    public DbSet<ProjectDesign> ProjectDesigns { get; set; }

    public DbSet<ProjectComponent> ProjectComponents { get; set; }

    public DbSet<ProjectComponentUnit> ProjectComponentUnits { get; set; }

    public DbSet<ProjectQuantity> ProjectQuantities { get; set; }

    public DbSet<ProjectScenario> ProjectScenarios { get; set; }

    public DbSet<ProjectScenarioUnit> ProjectScenarioUnits { get; set; }

    public DbSet<ProjectSimulation> ProjectSimulations { get; set; }

    public DbSet<ProjectSimulationEntry> ProjectSimulationEntries { get; set; }

    //
    // WIKI
    //

    public DbSet<WikiPage> WikiPages { get; set; }
}
