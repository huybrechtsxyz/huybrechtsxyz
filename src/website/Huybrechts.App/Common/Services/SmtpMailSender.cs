using Huybrechts.App.Config;
using Microsoft.Extensions.Configuration;
using MimeKit;

namespace Huybrechts.App.Common.Services;

public class SmtpMailSender : IEmailSender
{
	private readonly SmtpClientSecretOptions _SmtpClientLogin;
	private readonly SmtpServerOptions _mailSettings;
    private readonly Serilog.ILogger _logger;

    public SmtpMailSender(IConfiguration configuration, Serilog.ILogger logger)
	{
		ApplicationSettings settings = new(configuration);
		_SmtpClientLogin = settings.GetSmtpLoginClientSecret();
		_mailSettings = settings.GetSmtpServerSettings();
        _logger = logger;
    }

    /// <remarks>
    /// Using MailKit, Net.SmtpClient uses an older implementation
    /// </remarks>
    public async Task SendEmailAsync(string toEmail, string toName, string subject, string messageText)
	{
		try
		{
            _logger.Debug("Sending an email with MailKit to {to} with subject {subject}", toEmail, subject);
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
            if (!string.IsNullOrEmpty(_SmtpClientLogin.ClientId))
				await client.AuthenticateAsync(_SmtpClientLogin.ClientId, _SmtpClientLogin.ClientSecret);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
		catch (Exception ex)
		{
			throw new InvalidOperationException(ex.Message);
		}
	}
}
