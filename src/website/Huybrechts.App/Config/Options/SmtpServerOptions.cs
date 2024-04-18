using System.ComponentModel.DataAnnotations;

namespace Huybrechts.App.Config.Options;

public sealed class SmtpServerOptions
{
	[Required]
	public string Username { get; set; } = string.Empty;

	public string Password { get; set; } = string.Empty;

	[Required]
	public string MailServer { get; set; } = string.Empty;

	[Required]
	public int MailPort { get; set; } = 465;

	public bool EnableSsl { get; set; } = true;

	[Required]
	public string SenderMail { get; set; } = string.Empty;

	public string SenderName { get; set; } = string.Empty;
}
