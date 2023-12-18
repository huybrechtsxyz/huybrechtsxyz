using Huybrechts.Helpers;
using Huybrechts.Website.Data;
using Huybrechts.Website.Models;
using System.Net;
using System.Net.Mail;

namespace Huybrechts.Services;

public interface IEmailSender
{
	Task SendEmailAsync(string toEmail, string toName, string subject, string messageText);
}

public class MessageSender : IEmailSender
{
	private readonly MessagingAuthentication _messageAuthentication;
	private readonly MessagingSettings _messageSettings;

    public MessageSender(IConfiguration configuration)
	{
		_messageSettings = new ConfigurationHelper(configuration).GetMessagingSettings();
		_messageAuthentication = new ConfigurationHelper(configuration).GetMessagingAuthentication();
    }

	public async Task SendEmailAsync(string toEmail, string toName, string subject, string messageText)
	{
		try
		{
            SmtpClient client = new()
            {
                Port = _messageSettings.MailPort, // 587,
                Host = _messageSettings.MailServer, // "smtp.gmail.com" or another email sender provider
                EnableSsl = _messageSettings.EnableSsl, // true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(_messageAuthentication.UserName, _messageAuthentication.Password)
            };

			MailMessage mailMessage = new()
			{
				From = new MailAddress(_messageSettings.SenderMail, _messageSettings.SenderName),
				Subject = subject,
				Body = messageText,
			};

			mailMessage.To.Add(new MailAddress(toEmail, toName));

			await client.SendMailAsync(mailMessage);
			return;
        }
		catch (Exception ex)
		{
			throw new InvalidOperationException(ex.Message);
		}
	}
}
