using Huybrechts.Website.Helpers;
using Huybrechts.Website.Models;
using MimeKit;

namespace Huybrechts.Services;

public interface IEmailSender
{
	Task SendEmailAsync(string toEmail, string toName, string subject, string messageText);
}

public class MessageSender : IEmailSender
{
	private readonly MessageAuthenticationSettings _mailAuthentication;
	private readonly MessageServerSettings _mailSettings;

    public MessageSender(IConfiguration configuration)
	{
		ApplicationSettings settings = new(configuration);
        _mailSettings = settings.GetMessageServerSettings();
        _mailAuthentication = settings.GetMessageAuthenticationSettings();
    }

    /// <summary>
    /// Sends an e-mail throught the provided server settings
    /// </summary>
    /// <param name="toEmail"></param>
    /// <param name="toName"></param>
    /// <param name="subject"></param>
    /// <param name="messageText"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    /// <remarks>
    /// Using MailKit, Net.SmtpClient uses an older implementation
    /// </remarks>
    public async Task SendEmailAsync(string toEmail, string toName, string subject, string messageText)
	{
		try
		{
            using MimeMessage message = new();
			message.From.Add(new MailboxAddress(_mailSettings.SenderName, _mailSettings.SenderMail));
			message.To.Add(new MailboxAddress(toName, toEmail));
			message.Subject = subject;
			message.Body = new TextPart("html")
			{
				Text = messageText
			};

			using MailKit.Net.Smtp.SmtpClient client = new();
			client.ServerCertificateValidationCallback = (s, c, h, e) => true;
			await client.ConnectAsync(_mailSettings.MailServer, _mailSettings.MailPort, _mailSettings.EnableSsl);
            if (!string.IsNullOrEmpty(_mailAuthentication.Username))
				await client.AuthenticateAsync(_mailAuthentication.Username, _mailAuthentication.Password);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
		catch (Exception ex)
		{
			throw new InvalidOperationException(ex.Message);
		}
	}
}
