namespace Huybrechts.App.Services.Mail;

public interface IEmailServer
{
	Task SendEmailAsync(string toEmail, string toName, string subject, string messageText);
}
