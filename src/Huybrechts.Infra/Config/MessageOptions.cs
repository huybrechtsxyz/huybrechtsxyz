namespace Huybrechts.Infra.Config;

public class MessageServerOptions
{
	public string MailServer { get; set; } = string.Empty;

    public int MailPort { get; set; } = 0;

    public bool EnableSsl { get; set; } = false;

    public string SenderMail { get; set; } = string.Empty;

    public string SenderName { get; set; } = string.Empty;
}

public class MessageAuthenticationOptions
{
    public string Username { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;
}
