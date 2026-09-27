using Huybrechts.App.Data;
using Huybrechts.Core.Application;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Security.Claims;

namespace Huybrechts.App.Application;

public class ApplicationUserStore :
	UserStore<ApplicationUser, 
        ApplicationRole, 
        ApplicationContext, string,
        ApplicationUserClaim, 
        ApplicationUserRole, 
        ApplicationUserLogin, 
        ApplicationUserToken, 
        ApplicationRoleClaim>,
    IUserStore<ApplicationUser>
{
    private DbSet<ApplicationRole> Roles { get { return Context.Set<ApplicationRole>(); } }

    private DbSet<ApplicationUserRole> UserRoles { get { return Context.Set<ApplicationUserRole>(); } }

    public ApplicationUserStore(ApplicationContext context, IdentityErrorDescriber? describer = null)
        : base(context, describer)
    {
        //_dbcontext = context;
    }

    /// <summary>
    /// Retrieves the roles the specified <paramref name="user"/> is a member of.
    /// </summary>
    /// <param name="user">The user whose roles should be retrieved.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>A <see cref="Task{TResult}"/> that contains the roles the user is a member of.</returns>
    public async Task<IList<ApplicationRole>> GetApplicationRolesAsync(ApplicationUser user, CancellationToken cancellationToken = default)
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

	/// <summary>
	/// Get the single tenant from the database
	/// </summary>
	/// <param name="user">The user whose roles should be retrieved.</param>
	/// <param name="tenantId">The tenant Id for filtering the roles</param>
	/// <returns>Returns the tenant if the user has access to it or is an administrator</returns>
	public async Task<ApplicationTenant?> GetTenantAsync(ApplicationUser user, string tenantId)
	{
		var tenant = await Context.ApplicationTenants.FirstOrDefaultAsync(q => q.Id == tenantId);
		if (tenant == null)
			return null;

		if (await IsInTenantAsync(user.Id, tenantId))
			return tenant;

		if (await IsInRoleAsync(user, ApplicationRole.GetNormalizedRoleName(ApplicationSystemRole.Administrator)))
			return tenant;

		return null;
	}

	/// <summary>
	/// Retrieves the roles the specified <paramref name="user"/> is a member of.
	/// </summary>
	/// <param name="user">The user whose roles should be retrieved.</param
	/// <param name="tenantId">The tenant Id for filtering the roles</param>
	/// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
	/// <returns>A <see cref="Task{TResult}"/> that contains the roles the user is a member of.</returns>
	public async Task<IList<string>> GetRolesAsync(ApplicationUser user, string tenantId, CancellationToken cancellationToken = default)
	{
		cancellationToken.ThrowIfCancellationRequested();
		ThrowIfDisposed();
		ArgumentNullException.ThrowIfNull(user);
		var userId = user.Id;
		var query = from userRole in UserRoles
					join role in Roles on userRole.RoleId equals role.Id
					where userRole.UserId.Equals(userId)
                    && (userRole.TenantId == null || userRole.TenantId.Equals(string.Empty) || userRole.TenantId.Equals(tenantId) )
					select role.Name;
		return await query.ToListAsync(cancellationToken);
	}

    /// <summary>
    /// Does the tenant have other users that are owner for this tenant
    /// </summary>
    /// <param name="user">The user to check against</param>
    /// <param name="tenantId">The tenant to check against</param>
    /// <param name="cancellationToken"></param>
    /// <returns>If other owners are registered to this tenant</returns>
    public async Task<bool> HasOtherOwnersAsync(ApplicationUser user, string tenantId, CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(user);
        ArgumentException.ThrowIfNullOrWhiteSpace(tenantId);

        var roleId = ApplicationRole.GetRoleName(tenantId, ApplicationTenantRole.Owner);
        var userRoles = await UserRoles.Where(q => q.UserId != user.Id && q.RoleId == roleId).ToListAsync(cancellationToken);
        if (userRoles is null || userRoles.Count == 0) return true;
        return false;
    }

    /// <summary>
    /// Check if the user is a member of a tenant
    /// </summary>
    /// <param name="userId">User ID of the member</param>
    /// <param name="tenantId">Tenant ID of the tenant</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if the user is a member of the tenant</returns>
    public async Task<bool> IsInTenantAsync(string userId, string tenantId, CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentException.ThrowIfNullOrEmpty(userId);
        ArgumentException.ThrowIfNullOrEmpty(tenantId);

        var tenant = await Context.ApplicationTenants.FindAsync([tenantId], cancellationToken: cancellationToken);
        if (tenant is null)
            return false;

        return await UserRoles.AnyAsync(q => q.UserId == userId && q.TenantId == tenantId, cancellationToken);
    }

    /// <summary>
    /// Sets the given <paramref name="userName" /> for the specified <paramref name="user"/>.
    /// </summary>
    /// <param name="user">The user whose name should be set.</param>
    /// <param name="userName">The user name to set.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>The <see cref="Task"/> that represents the asynchronous operation.</returns>
    public Task SetUserNameAsync(ApplicationUser user, string? userName, string? givenName, string? surname, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(user);
        user.UserName = userName;
        user.GivenName = givenName;
        user.Surname = surname;
        return Task.CompletedTask;
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
            User = user,
            UserId = user.Id,
            Role = role,
            RoleId = role.Id,
            TenantId = role.TenantId
        };
    }

    /// <summary>
    /// Called to create a new instance of a <see cref="IdentityUserLogin{TKey}"/>.
    /// </summary>
    /// <param name="user">The associated user.</param>
    /// <param name="login">The associated login.</param>
    /// <returns></returns>
    protected override ApplicationUserLogin CreateUserLogin(ApplicationUser user, UserLoginInfo login)
    {
        return new ApplicationUserLogin
        {
            User = user,
            UserId = user.Id,
            ProviderKey = login.ProviderKey,
            LoginProvider = login.LoginProvider,
            ProviderDisplayName = login.ProviderDisplayName
        };
    }

    /// <summary>
    /// Called to create a new instance of a <see cref="IdentityUserClaim{TKey}"/>.
    /// </summary>
    /// <param name="user">The associated user.</param>
    /// <param name="claim">The associated claim.</param>
    /// <returns></returns>
    protected override ApplicationUserClaim CreateUserClaim(ApplicationUser user, Claim claim)
    {
        var userClaim = new ApplicationUserClaim { User = user, UserId = user.Id };
        userClaim.InitializeFromClaim(claim);
        return userClaim;
    }

    /// <summary>
    /// Called to create a new instance of a <see cref="IdentityUserToken{TKey}"/>.
    /// </summary>
    /// <param name="user">The associated user.</param>
    /// <param name="loginProvider">The associated login provider.</param>
    /// <param name="name">The name of the user token.</param>
    /// <param name="value">The value of the user token.</param>
    /// <returns></returns>
    protected override ApplicationUserToken CreateUserToken(ApplicationUser user, string loginProvider, string name, string? value)
    {
        return new ApplicationUserToken
        {
            User = user,
            UserId = user.Id,
            LoginProvider = loginProvider,
            Name = name,
            Value = value
        };
    }
}