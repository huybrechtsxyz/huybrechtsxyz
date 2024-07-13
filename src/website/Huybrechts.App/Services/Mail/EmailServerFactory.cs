using Huybrechts.App.Config;

namespace Huybrechts.App.Services.Mail;

public static class EmailServerFactory
{
    public static IEmailServer Create(SmtpServerOptions options, Serilog.ILogger logger)
    {
        if (options == null || string.IsNullOrEmpty(options.MailServer))
            return new MockEmailServer(logger);
        return new SmtpEmailServer(options, logger);
    }
}
