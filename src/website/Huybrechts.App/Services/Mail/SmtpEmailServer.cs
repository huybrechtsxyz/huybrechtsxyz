using Huybrechts.App.Config;
using MimeKit;

namespace Huybrechts.App.Services.Mail;

public class SmtpEmailServer : IEmailServer
{
    private readonly SmtpServerOptions _mailSettings;
    private readonly Serilog.ILogger _logger;

    public SmtpEmailServer(SmtpServerOptions smtpServerOptions, Serilog.ILogger logger)
    {
        _mailSettings = smtpServerOptions;
        _logger = logger;
    }

    /// <remarks>
    /// Using MailKit, Net.SmtpClient uses an older implementation
    /// </remarks>
    public async Task SendEmailAsync(string toEmail, string toName, string subject, string messageText)
    {
        try
        {
            _logger.Debug("Sending an email with MailKit {sendMailFrom} to {sendMailTo} with subject {sendMailSubject} (", _mailSettings.MailServer, toEmail, subject);
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
            if (!string.IsNullOrEmpty(_mailSettings.Username))
                await client.AuthenticateAsync(_mailSettings.Username, _mailSettings.Password);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(ex.Message);
        }
    }
}
