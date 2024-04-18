using Huybrechts.App.Common.Services;
using Huybrechts.App.Config.Options;
using Huybrechts.App.Identity.Entities;
using Microsoft.Extensions.Options;

namespace Huybrechts.App.Identity.Services;

public class AuthenticationSender : Microsoft.AspNetCore.Identity.IEmailSender<ApplicationUser>
{
    private readonly IEmailSender emailSender;

    public AuthenticationSender(IOptions<SmtpServerOptions> options, Serilog.ILogger logger)
    {
        emailSender = new SmtpMailSender(options.Value, logger);
    }

    public AuthenticationSender(IEmailSender sender)
    {
        emailSender = sender;
    }

    public async Task SendConfirmationLinkAsync(ApplicationUser user, string email, string confirmationLink)
    {
        await emailSender.SendEmailAsync(user.Email!, user.Fullname, "Confirm your account", "Please confirm your account at: " + confirmationLink);
    }

    public async Task SendPasswordResetCodeAsync(ApplicationUser user, string email, string resetCode)
    {
        await emailSender.SendEmailAsync(user.Email!, user.Fullname, "Reset your account", "Please reset your password with code: " + resetCode);
    }

    public async Task SendPasswordResetLinkAsync(ApplicationUser user, string email, string resetLink)
    {
        await emailSender.SendEmailAsync(user.Email!, user.Fullname, "Reset your account", "Please reset your password at: " + resetLink);
    }
}
