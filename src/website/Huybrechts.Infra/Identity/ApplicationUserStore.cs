using Huybrechts.Core.Identity.Entities;
using Huybrechts.Infra.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace Huybrechts.Infra.Identity;

public class ApplicationUserStore : UserStore<ApplicationUser>, IUserStore<ApplicationUser>
{
    private readonly DatabaseContext _dbcontext;

    private DbSet<ApplicationRole> Roles { get { return Context.Set<ApplicationRole>(); } }

    private DbSet<ApplicationTenant> Tenants { get { return Context.Set<ApplicationTenant>(); } }

    private DbSet<ApplicationUserRole> UserRoles { get { return Context.Set<ApplicationUserRole>(); } }

    private DbSet<ApplicationUserTenant> UserTenants { get { return Context.Set<ApplicationUserTenant>(); } }

    public ApplicationUserStore(DatabaseContext context, IdentityErrorDescriber? describer = null)
        : base(context, describer)
    {
        _dbcontext = context;
    }

    public async Task<IList<ApplicationRole>> GetAppliationRolesAsync(ApplicationUser user, CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(user);
        var userId = user.Id;
        var query = from userRoles in UserRoles
                    join role in Roles on userRoles.RoleId equals role.Id
                    where userRoles.UserId.Equals(userId)
                    select role;
        return await query.ToListAsync(cancellationToken);
    }

    public async Task AddToTenantAsync(ApplicationUser user, string tenantId, CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(user, nameof(user));
        ArgumentException.ThrowIfNullOrWhiteSpace(tenantId, nameof(tenantId));

        var tenant = await Tenants.FindAsync(tenantId, cancellationToken) ??
            throw new InvalidOperationException($"Tenant {tenantId} was not found");

        var userTenant = await UserTenants.FirstAsync(q => q.UserId == user.Id && tenantId == tenant.Id);
        if (userTenant is not null)
            return;

        UserTenants.Add(new ApplicationUserTenant()
        {
            UserId = user.Id,
            TenantId = tenantId
        });
    }
    
    public async Task<IList<string>> GetTenantsAsync(ApplicationUser user, CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(user);
        var userId = user.Id;
        var query = from userTenant in UserTenants
                    join tenant in Tenants on userTenant.TenantId equals tenant.Id
                    where userTenant.UserId.Equals(userId)
                    select tenant.Name;
        return await query.ToListAsync(cancellationToken);
    }

    public async Task<IList<ApplicationTenant>> GetApplicationTenantsAsync(ApplicationUser user, CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(user);
        var userId = user.Id;
        var query = from userTenant in UserTenants
                    join tenant in Tenants on userTenant.TenantId equals tenant.Id
                    where userTenant.UserId.Equals(userId)
                    select tenant;
        return await query.ToListAsync(cancellationToken);
    }

    public async Task<IList<ApplicationUser>> GetUsersInTenantAsync(string tenantId, CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentException.ThrowIfNullOrWhiteSpace(tenantId);
        var tenant = await Tenants.FindAsync([tenantId, cancellationToken], cancellationToken: cancellationToken);
        if (tenant is null)
            return new List<ApplicationUser>();
        var query = from userTenant in UserTenants
                    join user in Users on userTenant.UserId equals user.Id
                    where userTenant.TenantId.Equals(tenant.Id)
                    select user;
        return await query.ToListAsync(cancellationToken);
    }

    public async Task<bool> IsInTenantAsync(ApplicationUser user, string tenantId, CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(user);
        ArgumentException.ThrowIfNullOrWhiteSpace(tenantId);

        var tenant = await Tenants.FindAsync([tenantId, cancellationToken], cancellationToken: cancellationToken);
        if (tenant is null)
            return false;

        return await UserTenants.AnyAsync(q => q.UserId == user.Id && q.TenantId == tenantId, cancellationToken: cancellationToken);
    }

    public async Task RemoveFromTenantAsync(ApplicationUser user, string tenantId, CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(user);
        ArgumentException.ThrowIfNullOrWhiteSpace(tenantId);

        var tenants = await UserTenants.Where(q => q.UserId == user.Id && q.TenantId == tenantId).ToListAsync(cancellationToken);
        if (tenants is null || tenants.Count == 0)
            return;

        UserTenants.RemoveRange(tenants);
        
        var roles = await UserRoles.Where(q => q.UserId == user.Id && q.TenantId != tenantId).ToListAsync();
        if (roles is not null && roles.Count > 0)
            UserRoles.RemoveRange(roles);
    }

    public override async Task<IdentityResult> UpdateAsync(ApplicationUser user, CancellationToken cancellationToken = default)
    {
        var result = await base.UpdateAsync(user, cancellationToken);
        if (!result.Succeeded)
            return result;
        await _dbcontext.SaveChangesAsync(cancellationToken);
        return IdentityResult.Success;
    }
}
