using Huybrechts.Services;
using Huybrechts.Website.Data;

namespace Huybrechts.Website.Services;
public class AuthenticationSender : Microsoft.AspNetCore.Identity.IEmailSender<ApplicationUser>
{
    private readonly MessageSender messageSender;

    public AuthenticationSender(IConfiguration configuration)
    {
        messageSender = new MessageSender(configuration);
    }

    public async Task SendConfirmationLinkAsync(ApplicationUser user, string email, string confirmationLink)
    {
        await messageSender.SendEmailAsync(user.Email!, user.Fullname, "Confirm your account", "Please confirm your account at: " + confirmationLink);
    }

    public async Task SendPasswordResetCodeAsync(ApplicationUser user, string email, string resetCode)
    {
        await messageSender.SendEmailAsync(user.Email!, user.Fullname, "Reset your account", "Please reset your password with code: " + resetCode);
    }

    public async Task SendPasswordResetLinkAsync(ApplicationUser user, string email, string resetLink)
    {
        await messageSender.SendEmailAsync(user.Email!, user.Fullname, "Reset your account", "Please reset your password at: " + resetLink);
    }
}
