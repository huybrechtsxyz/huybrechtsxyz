namespace Huybrechts.App.Services.Mail;

public interface IEmailSender
{
	Task SendEmailAsync(string toEmail, string toName, string subject, string messageText);
}
