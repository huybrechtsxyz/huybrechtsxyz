using Microsoft.Extensions.Logging;

namespace Huybrechts.Infra.Services;

public class MockEmailSender : IEmailSender
{
    private readonly ILogger _logger;

    public MockEmailSender(ILogger logger)
    {
        _logger = logger;            
    }

    public Task SendEmailAsync(string toEmail, string toName, string subject, string messageText)
    {
        _logger.LogInformation("Sending an email with Mock to {to} with subject {subject}", toEmail, subject);
        return Task.CompletedTask;
    }
}
