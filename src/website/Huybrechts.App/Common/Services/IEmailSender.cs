namespace Huybrechts.App.Common.Services;

public interface IEmailSender
{
	Task SendEmailAsync(string toEmail, string toName, string subject, string messageText);
}
