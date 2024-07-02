using Huybrechts.Core.Application;
using Huybrechts.Infra.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
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

    public override async Task<IdentityResult> CreateAsync(ApplicationUser user, CancellationToken cancellationToken = default)
    {
        var result = await base.CreateAsync(user, cancellationToken);
        if (!result.Succeeded)
            return result;

        await base.AddToRoleAsync(user, ApplicationRole.GetRoleName(ApplicationDefaultSystemRole.User), cancellationToken);
        return result;
    }

    /// <summary>
    /// Retrieves the roles the specified <paramref name="user"/> is a member of.
    /// </summary>
    /// <param name="user">The user whose roles should be retrieved.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>A <see cref="Task{TResult}"/> that contains the roles the user is a member of.</returns>
    public async Task<IList<ApplicationRole>> GetApplicationRolesAsync(ApplicationUser user, CancellationToken cancellationToken = default(CancellationToken))
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(user);
        var userId = user.Id;
        var query = from userRole in UserRoles
                    join role in Roles on userRole.RoleId equals role.Id
                    where userRole.UserId.Equals(userId)
                    select role;
        return await query.ToListAsync(cancellationToken);
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
                    where userRole.UserId.Equals(userId) && tenant.State != ApplicationTenantState.Removed
                    select tenant;
        return await query.GroupBy(q => q.Name).Select(q => q.First()).ToListAsync(cancellationToken);
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

        var roleId = ApplicationRole.GetRoleName(tenantId, ApplicationDefaultTenantRole.Owner);
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

        var roleId = ApplicationRole.GetRoleName(tenantId, ApplicationDefaultTenantRole.Owner);
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
    public Task SetGivenAndSurnameAsync(ApplicationUser user, string? givenName, string? surname, CancellationToken cancellationToken = default)
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

    /// <summary>
    /// Called to create a new instance of a <see cref="IdentityUserRole{TKey}"/>.
    /// </summary>
    /// <param name="user">The associated user.</param>
    /// <param name="role">The associated role.</param>
    /// <returns></returns>
    protected override ApplicationUserRole CreateUserRole(ApplicationUser user, ApplicationRole role)
    {
        return new ApplicationUserRole()
        {
            UserId = user.Id,
            RoleId = role.Id,
            TenantId = role.TenantId
        };
    }
}