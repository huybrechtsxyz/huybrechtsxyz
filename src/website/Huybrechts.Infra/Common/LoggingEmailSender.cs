namespace Huybrechts.Infra.Common;

public class LoggingEmailSender : IEmailSender
{
    private readonly Serilog.ILogger _logger;

    public LoggingEmailSender(Serilog.ILogger logger)
    {
        _logger = logger;            
    }

    public Task SendEmailAsync(string toEmail, string toName, string subject, string messageText)
    {
        _logger.Information("Sending an mock email to {to} with subject {subject}", toEmail, subject);
        return Task.CompletedTask;
    }
}
