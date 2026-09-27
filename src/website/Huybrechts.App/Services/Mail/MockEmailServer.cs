namespace Huybrechts.App.Services.Mail;

public class MockEmailServer : IEmailServer
{
    private readonly Serilog.ILogger _logger;

    public MockEmailServer(Serilog.ILogger logger)
    {
        _logger = logger;            
    }

    public Task SendEmailAsync(string toEmail, string toName, string subject, string messageText)
    {
        _logger.Information("Sending an email with Mock to {to} with subject {subject}", toEmail, subject);
        return Task.CompletedTask;
    }
}
