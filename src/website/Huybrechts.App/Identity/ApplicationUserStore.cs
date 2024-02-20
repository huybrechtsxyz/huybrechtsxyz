using Huybrechts.App.Identity.Entities;
using Huybrechts.App.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace Huybrechts.App.Identity;

public class ApplicationUserStore :
	UserStore<ApplicationUser, ApplicationRole, DatabaseContext, string, ApplicationUserClaim, ApplicationUserRole, ApplicationUserLogin, ApplicationUserToken, ApplicationRoleClaim>,
    IUserStore<ApplicationUser>
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

    public async Task AddToTenantAsync(ApplicationUser user, string tenantId, string remark = "", CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(user, nameof(user));
        ArgumentException.ThrowIfNullOrWhiteSpace(tenantId, nameof(tenantId));

        var tenant = await Tenants.FindAsync([tenantId], cancellationToken: cancellationToken) ??
            throw new InvalidOperationException($"Tenant {tenantId} was not found while adding user to tenant");
        
        var userTenant = await UserTenants.FirstAsync(q => q.UserId == user.Id && tenantId == tenant.Id, cancellationToken: cancellationToken);
        if (userTenant is null)
        {
            userTenant = new ApplicationUserTenant()
            {
                UserId = user.Id,
                TenantId = tenantId,
                Remark = remark
            };
            UserTenants.Add(userTenant);
        }
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
        var tenant = await Tenants.FindAsync([tenantId], cancellationToken: cancellationToken);
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

        var tenant = await Tenants.FindAsync([tenantId], cancellationToken: cancellationToken);
        if (tenant is null)
            return false;

        return await UserTenants.AnyAsync(q => q.UserId == user.Id && q.TenantId == tenantId, cancellationToken);
    }

    public async Task<bool> IsTenantOwnerAsync(ApplicationUser user, string tenantId, CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(user);
        ArgumentException.ThrowIfNullOrWhiteSpace(tenantId);

        var roleId = ApplicationRole.GetTenantRole(tenantId, ApplicationRoleValues.Owner);
        var userRoles = await UserRoles.Where(q => q.UserId == user.Id && q.RoleId == roleId).ToListAsync(cancellationToken);
        if (userRoles is null || userRoles.Count == 0) return false;
        return true;
    }

    public async Task<bool> HasOtherOwnersAsync(ApplicationUser user, string tenantId, CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(user);
        ArgumentException.ThrowIfNullOrWhiteSpace(tenantId);

        var roleId = ApplicationRole.GetTenantRole(tenantId, ApplicationRoleValues.Owner);
        var userRoles = await UserRoles.Where(q => q.UserId != user.Id && q.RoleId == roleId).ToListAsync(cancellationToken);
        if (userRoles is null || userRoles.Count == 0) return true;
        return false;
    }

    public async Task RemoveFromTenantAsync(ApplicationUser user, string tenantId, CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(user);
        ArgumentException.ThrowIfNullOrWhiteSpace(tenantId);

        if (await IsTenantOwnerAsync(user, tenantId, cancellationToken))
            if (!await HasOtherOwnersAsync(user, tenantId, cancellationToken))
                throw new ApplicationException($"Tenant {tenantId} has no other owners than {user.Id}");

        var roles = await UserRoles.Where(q => q.UserId == user.Id && q.TenantId == tenantId).ToListAsync(cancellationToken);
        if (roles is not null && roles.Count > 0)
            UserRoles.RemoveRange(roles);

        var tenants = await UserTenants.Where(q => q.UserId == user.Id && q.TenantId == tenantId).ToListAsync(cancellationToken);
        if (tenants is not null && tenants.Count != 0)
            UserTenants.RemoveRange(tenants);
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