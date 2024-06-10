using Huybrechts.Core.Application;
using Huybrechts.Infra.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Crypto;
using System.Data;

namespace Huybrechts.Infra.Application;

public class ApplicationUserStore :
	UserStore<ApplicationUser, ApplicationRole, ApplicationContext, string, ApplicationUserClaim, ApplicationUserRole, ApplicationUserLogin, ApplicationUserToken, ApplicationRoleClaim>,
    IUserStore<ApplicationUser>
{
    private readonly ApplicationContext _dbcontext;

    private DbSet<ApplicationRole> Roles { get { return Context.Set<ApplicationRole>(); } }

    private DbSet<ApplicationTenant> Tenants { get { return Context.Set<ApplicationTenant>(); } }

    private DbSet<ApplicationUserRole> UserRoles { get { return Context.Set<ApplicationUserRole>(); } }

    public ApplicationUserStore(ApplicationContext context, IdentityErrorDescriber? describer = null)
        : base(context, describer)
    {
        _dbcontext = context;
    }

    public async Task<ApplicationUser?> GetUserAsync(string userid)
    {
        return await _dbcontext.Users.FindAsync(userid);
    }

    public async Task<IList<string>> GetTenantNamesAsync(ApplicationUser user, CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(user);
        var userId = user.Id;
        var query = from userRole in UserRoles
                    join tenant in Tenants on userRole.TenantId equals tenant.Id
                    where userRole.UserId.Equals(userId)
                    select tenant.Name;
        return await query.Distinct().ToListAsync(cancellationToken);
    }

    public async Task<IList<ApplicationTenant>> GetTenantsListAsync(ApplicationUser user, CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(user);
        var userId = user.Id;
        var query = from userRole in UserRoles
                    join tenant in Tenants on userRole.TenantId equals tenant.Id
                    where userRole.UserId.Equals(userId)
                    select tenant;
        return (IList<ApplicationTenant>)await query.GroupBy(q => q.Name).ToListAsync(cancellationToken);
    }

    public async Task<IList<ApplicationUser>> GetUsersInTenantAsync(string tenantId, CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentException.ThrowIfNullOrWhiteSpace(tenantId);
        var tenant = await Tenants.FindAsync([tenantId], cancellationToken: cancellationToken);
        if (tenant is null)
            return new List<ApplicationUser>();
        var query = from userRole in UserRoles
                    join user in Users on userRole.UserId equals user.Id
                    where userRole.TenantId == tenant.Id
                    select user;
        return (IList<ApplicationUser>)await query.GroupBy(q => q.NormalizedUserName).ToListAsync(cancellationToken);
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

        return await UserRoles.AnyAsync(q => q.UserId == user.Id && q.TenantId == tenantId, cancellationToken);
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
    }

    /// <summary>
    /// Sets the given <paramref name="userName" /> for the specified <paramref name="user"/>.
    /// </summary>
    /// <param name="user">The user whose name should be set.</param>
    /// <param name="givenName">The given name to set.</param>
    /// <param name="surname">The surname to set.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>The <see cref="Task"/> that represents the asynchronous operation.</returns>
    public virtual Task SetGivenAndSurnameAsync(ApplicationUser user, string? givenName, string? surname, CancellationToken cancellationToken = default(CancellationToken))
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(user);
        user.GivenName = givenName;
        user.Surname = surname;
        return Task.CompletedTask;
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