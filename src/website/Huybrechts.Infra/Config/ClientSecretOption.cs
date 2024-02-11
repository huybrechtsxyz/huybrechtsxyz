namespace Huybrechts.Infra.Config;

public sealed class GoogleClientSecretOptions : ClientSecretOption { public const string GoogleClientSecret = "Authentication:GoogleLogin"; }

public sealed class SmtpClientSecretOptions: ClientSecretOption { public const string SmtpClientSecret = "Authentication:SmtpLogin"; }

public abstract class ClientSecretOption
{
    public virtual string ClientId { get; set; } = string.Empty;

    public virtual string ClientSecret { get; set; } = string.Empty;
}
