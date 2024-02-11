namespace Huybrechts.Infra.Services;

public interface IEmailSender
{
	Task SendEmailAsync(string toEmail, string toName, string subject, string messageText);
}
