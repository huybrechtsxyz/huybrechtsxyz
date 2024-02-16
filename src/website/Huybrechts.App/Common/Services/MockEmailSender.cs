namespace Huybrechts.App.Common.Services;

public class MockEmailSender : IEmailSender
{
    private readonly Serilog.ILogger _logger;

    public MockEmailSender(Serilog.ILogger logger)
    {
        _logger = logger;            
    }

    public Task SendEmailAsync(string toEmail, string toName, string subject, string messageText)
    {
        _logger.Information("Sending an email with Mock to {to} with subject {subject}", toEmail, subject);
        return Task.CompletedTask;
    }
}
