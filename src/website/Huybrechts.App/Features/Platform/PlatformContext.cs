using Finbuckle.MultiTenant.Abstractions;
using Finbuckle.MultiTenant.EntityFrameworkCore;
using Huybrechts.Core.Platform;
using Microsoft.EntityFrameworkCore;

// Add a blog for a tenant.
//Blog myBlog = new Blog { Title = "My Blog" }; ;
//var db = new BloggingDbContext(myTenantInfo, null);
//db.Blogs.Add(myBlog));
//db.SaveChanges();

namespace Huybrechts.App.Features.Platform;

public class PlatformContext : MultiTenantDbContext, IMultiTenantDbContext
{
    //public PlatformContext(ITenantInfo? tenantInfo) : base(tenantInfo)
    //{
    //}
    //public PlatformContext(IMultiTenantContextAccessor multiTenantContextAccessor) : base(multiTenantContextAccessor)
    //{
    //}
    //public PlatformContext(ITenantInfo? tenantInfo, DbContextOptions options) 
    //    : base(tenantInfo, options)
    //{
    //}

    public PlatformContext(IMultiTenantContextAccessor multiTenantContextAccessor, DbContextOptions options) : base(multiTenantContextAccessor, options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // set a global query filter, e.g. to support soft delete
        //modelBuilder.Entity<Post>().HasQueryFilter(p => !p.IsDeleted);

        // call the base library implementation AFTER the above
        base.OnModelCreating(modelBuilder);
    }

    public DbSet<PlatformInfo> Platforms { get; set; }

    public DbSet<PlatformLocation> Locations { get; set; }

    public DbSet<PlatformMeasureUnit> MeasureUnits { get; set; }

    public DbSet<PlatformMeasureDefault> MeasureDefaults { get; set; }

    public DbSet<PlatformService> Services { get; set; }

    public DbSet<PlatformResource> Resources { get; set; }

    public DbSet<PlatformRate> Rates { get; set; }

    public DbSet<PlatformRateUnit> RateUnits { get; set; }

    public DbSet<PlatformSearchRate> SearchRates { get; set; }
}
