using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Huybrechts.App.Config;

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

	public string ToLogString() 
	{ 
		var builder = new StringBuilder();
		builder.AppendLine("SmtpServerOptions");
		builder.Append("    Username: ").AppendLine(Username);
		builder.Append("    Password: ").AppendLine(Password);
		builder.Append("    MailServer: ").AppendLine(MailServer);
		builder.Append("    MailPort: ").AppendLine(MailPort.ToString());
		builder.Append("    EnableSsl: ").AppendLine(EnableSsl.ToString());
		builder.Append("    SenderMail: ").AppendLine(SenderMail);
		builder.Append("    SenderName: ").AppendLine(SenderName);
		return builder.ToString();
	}
}
