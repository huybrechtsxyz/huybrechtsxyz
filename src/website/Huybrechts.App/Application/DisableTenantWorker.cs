using Huybrechts.App.Config;
using Huybrechts.App.Data;
using Huybrechts.App.Services.Mail;
using Huybrechts.Core.Application;

namespace Huybrechts.App.Application;

public class DisableTenantWorker
{
    private readonly ApplicationUserManager _userManager;
    private readonly ApplicationContext _dbcontext;
    private readonly IEmailSender _emailSender;

    public DisableTenantWorker(ApplicationUserManager userManager, ApplicationContext dbcontext, SmtpServerOptions options, Serilog.ILogger logger)
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

        if (!ApplicationTenantManager.AllowDisableTenant(tenant.State))
            return;

        tenant.State = ApplicationTenantState.Disabled;
        _dbcontext.ApplicationTenants.Update(tenant);
        await _dbcontext.SaveChangesAsync();
    }

    //private async Task SendTenantDeletedAsync(ApplicationUser user, ApplicationTenant tenant)
    //{
    //    await _emailSender.SendEmailAsync(user.Email!, user.Fullname, "Deleted your tenant", "Confirmation your tenant {tenant.Id} was deleted");
    //}
}
