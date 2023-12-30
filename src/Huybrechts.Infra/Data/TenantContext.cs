using Microsoft.EntityFrameworkCore;

namespace Huybrechts.Infra.Data;

public class TenantContext : DbContext
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