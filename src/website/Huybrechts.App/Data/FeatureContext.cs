using Finbuckle.MultiTenant.Abstractions;
using Finbuckle.MultiTenant.EntityFrameworkCore;
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
}
