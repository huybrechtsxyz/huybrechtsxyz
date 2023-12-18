using Huybrechts.Helpers;
using Huybrechts.Website.Models;
using MimeKit;

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
		_messageSettings = new SettingHelper(configuration).GetMessagingSettings();
		_messageAuthentication = new SettingHelper(configuration).GetMessagingAuthentication();
    }

	public async Task SendEmailAsync(string toEmail, string toName, string subject, string messageText)
	{
		try
		{
			/* Is using an older implementation using mailkit instead
            using SmtpClient client = new()
            {
                Port = _messageSettings.MailPort, // 587,
                Host = _messageSettings.MailServer, // "smtp.gmail.com" or another email sender provider
                EnableSsl = _messageSettings.EnableSsl, // true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(_messageAuthentication.UserName, _messageAuthentication.Password),
				Timeout = 10000
            };

			using MailMessage mailMessage = new()
			{
				From = new MailAddress(_messageSettings.SenderMail, _messageSettings.SenderName),
				Subject = subject,
				Body = messageText,
			};

			mailMessage.To.Add(new MailAddress(toEmail, toName));

			await client.SendMailAsync(mailMessage);
			
			return;*/

			using MimeMessage message = new();
			message.From.Add(new MailboxAddress(_messageSettings.SenderName, _messageSettings.SenderMail));
			message.To.Add(new MailboxAddress(toName, toEmail));
			message.Subject = subject;
			message.Body = new TextPart("html")
			{
				Text = messageText
			};

			using MailKit.Net.Smtp.SmtpClient client = new();
			client.ServerCertificateValidationCallback = (s, c, h, e) => true;
			await client.ConnectAsync(_messageSettings.MailServer, _messageSettings.MailPort, _messageSettings.EnableSsl);
            if (!string.IsNullOrEmpty(_messageAuthentication.UserName))
				await client.AuthenticateAsync(_messageAuthentication.UserName, _messageAuthentication.Password);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
		catch (Exception ex)
		{
			throw new InvalidOperationException(ex.Message);
		}
	}
}
