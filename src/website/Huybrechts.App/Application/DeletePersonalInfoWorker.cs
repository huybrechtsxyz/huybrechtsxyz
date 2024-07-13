using Huybrechts.App.Config;
using Huybrechts.App.Services.Mail;
using Huybrechts.Core.Application;

namespace Huybrechts.App.Application;

public class DeletePersonalInfoWorker
{
    private readonly ApplicationUserManager _userManager;
    private readonly IEmailServer _emailSender;

    public DeletePersonalInfoWorker(ApplicationUserManager userManager, SmtpServerOptions options, Serilog.ILogger logger)
    {
        _userManager = userManager;
        _emailSender = EmailServerFactory.Create(options, logger);
    }

    public async Task StartAsync(string? userId)
    {
        ArgumentException.ThrowIfNullOrEmpty(userId);
        var user = await _userManager.FindByIdAsync(userId) ??
            throw new InvalidOperationException($"Unexpected error occurred deleting user.");

        var result = await _userManager.DeleteAsync(user);
        if (!result.Succeeded)
        {
            throw new InvalidOperationException($"Unexpected error occurred deleting user.");
        }

        await SendAccountDeletedAsync(user);
    }

    private async Task SendAccountDeletedAsync(ApplicationUser user)
    {
        await _emailSender.SendEmailAsync(user.Email!, user.Fullname, "Deleted your account", $"Confirmation your account for {user.Email} was deleted");
    }
}
