using Huybrechts.App.Config;
using Huybrechts.App.Data;
using Huybrechts.App.Services.Mail;
using Huybrechts.Core.Application;

namespace Huybrechts.App.Application;

public class DeleteTenantWorker
{
    private readonly ApplicationUserManager _userManager;
    private readonly ApplicationContext _dbcontext;
    private readonly IEmailSender _emailSender;

    public DeleteTenantWorker(ApplicationUserManager userManager, ApplicationContext dbcontext, SmtpServerOptions options, Serilog.ILogger logger)
    {
        _userManager = userManager;
        _dbcontext = dbcontext;
        _emailSender = new SmtpMailSender(options, logger);
    }

    public async Task StartAsync(string userId, string tenantId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(userId);
        ArgumentException.ThrowIfNullOrEmpty(tenantId);

        var user = await _userManager.FindByIdAsync(userId) ??
            throw new InvalidOperationException($"Unable to find user {userId}.");

        var tenant = await _dbcontext.ApplicationTenants.FindAsync(tenantId);
        if (tenant is null)
            return;

        if (!ApplicationTenantManager.AllowRemoveTenant(tenant.State))
            return;

        var roles = _dbcontext.Roles.Where(q => q.TenantId == tenant.Id);
        if (roles.Any())
        {
            _dbcontext.RemoveRange(roles);
            await _dbcontext.SaveChangesAsync();
        }

        var userRoles = _dbcontext.UserRoles.Where(q => q.TenantId == tenant.Id);
        if (userRoles.Any())
        {
            _dbcontext.RemoveRange(userRoles);
            await _dbcontext.SaveChangesAsync();
        }

        tenant.State = ApplicationTenantState.Removed;
        _dbcontext.ApplicationTenants.Update(tenant);
        await _dbcontext.SaveChangesAsync();

        _dbcontext.ApplicationTenants.Remove(tenant);
        await _dbcontext.SaveChangesAsync();
    }

    //private async Task SendTenantDeletedAsync(ApplicationUser user, ApplicationTenant tenant)
    //{
    //    await _emailSender.SendEmailAsync(user.Email!, user.Fullname, "Deleted your tenant", "Confirmation your tenant {tenant.Id} was deleted");
    //}
}
