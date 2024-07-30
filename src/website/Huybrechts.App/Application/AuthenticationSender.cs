using Huybrechts.App.Config;
using Huybrechts.App.Services.Mail;
using Huybrechts.Core.Application;

namespace Huybrechts.App.Application;

public class AuthenticationMailer : Microsoft.AspNetCore.Identity.UI.Services.IEmailSender
{
    private readonly IEmailServer emailSender;

    public AuthenticationMailer(SmtpServerOptions options, Serilog.ILogger logger)
    {
        emailSender = EmailServerFactory.Create(options, logger);
    }

    public AuthenticationMailer(IEmailServer sender)
    {
        emailSender = sender;
    }

    public async Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        await emailSender.SendEmailAsync(email, email, subject, htmlMessage);
    }
}

public class AuthenticationSender : Microsoft.AspNetCore.Identity.IEmailSender<ApplicationUser>
{
    private readonly IEmailServer emailSender;

    public AuthenticationSender(SmtpServerOptions options, Serilog.ILogger logger)
    {
        emailSender = EmailServerFactory.Create(options, logger);
    }

    public AuthenticationSender(IEmailServer sender)
    {
        emailSender = sender;
    }

    public async Task SendConfirmationLinkAsync(ApplicationUser user, string email, string confirmationLink)
    {
        await emailSender.SendEmailAsync(user.Email!, user.Fullname, ApplicationLocalization.ConfirmAccount_Subject, ApplicationLocalization.ConfirmAccount_Body.Replace("{0}", confirmationLink));
    }

    public async Task SendPasswordResetCodeAsync(ApplicationUser user, string email, string resetCode)
    {
        await emailSender.SendEmailAsync(user.Email!, user.Fullname, ApplicationLocalization.ResetAccount_Subject, ApplicationLocalization.ResetAccount_Body.Replace("{0}", resetCode));
    }

    public async Task SendPasswordResetLinkAsync(ApplicationUser user, string email, string resetLink)
    {
        await emailSender.SendEmailAsync(user.Email!, user.Fullname, ApplicationLocalization.ResetPassword_Subject, ApplicationLocalization.ResetPassword_Subject.Replace("{0}", resetLink));
    }
}
