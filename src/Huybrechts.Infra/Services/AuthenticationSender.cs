using Huybrechts.Infra.Data;
using Microsoft.Extensions.Configuration;

namespace Huybrechts.Infra.Services;

public class AuthenticationSender : Microsoft.AspNetCore.Identity.IEmailSender<ApplicationUser>
{
    private readonly IEmailSender emailSender;

    public AuthenticationSender(IConfiguration configuration, Serilog.ILogger logger)
    {
        emailSender = new SmtpMailSender(configuration, logger);
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
