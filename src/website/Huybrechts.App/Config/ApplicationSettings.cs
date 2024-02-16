using Huybrechts.App.Data;
using Microsoft.Extensions.Configuration;
using System.Globalization;

namespace Huybrechts.App.Config;

public sealed class ApplicationSettings
{
	private readonly IConfiguration _configuration;

	public static readonly string ENV_DOTNET_RUNNING_IN_CONTAINER = "DOTNET_RUNNING_IN_CONTAINER";

    public static bool IsRunningInContainer() => (System.Environment.GetEnvironmentVariable(ENV_DOTNET_RUNNING_IN_CONTAINER) == "true");

	public ApplicationSettings(IConfiguration configuration)
	{
		_configuration = configuration;
	}

	public bool DoInitializeEnvironment() => GetEnvironmentInitialization() == EnvironmentInitialization.Initialize;

	public bool DoResetEnvironment() => GetEnvironmentInitialization() == EnvironmentInitialization.Reset;

	public string GetApplicationConnectionString()
	{
		return _configuration.GetConnectionString("DatabaseContext") ??
		throw new InvalidOperationException("Connection string 'DatabaseContext' not found.");
	}

	public DatabaseProviderType GetApplicationConnectionType()
	{
		if (Enum.TryParse<DatabaseProviderType>(_configuration["Environment:DatabaseType"], out DatabaseProviderType dbtype))
			return dbtype;
		throw new InvalidCastException("Invalid Environment:DatabaseType for type of DatabaseProviderType");
	}

	public string GetDefaultPassword()
	{
		return _configuration.GetValue<string>("Authentication:DefaultPassword") ?? string.Empty;
	}

	public GoogleClientSecretOptions GetGoogleLoginClientSecret()
	{
		GoogleClientSecretOptions item = new();
		_configuration.GetSection(GoogleClientSecretOptions.GoogleClientSecret).Bind(item);
		return item;
	}

	public SmtpClientSecretOptions GetSmtpLoginClientSecret()
	{
		SmtpClientSecretOptions item = new();
		_configuration.GetSection(SmtpClientSecretOptions.SmtpClientSecret).Bind(item);
		return item;
	}

	public SmtpServerOptions GetSmtpServerSettings()
	{
		SmtpServerOptions item = new();
		_configuration.GetSection(SmtpServerOptions.SmtpServer).Bind(item);
		return item;
	}

	public static CultureInfo[] GetSupportedCultures() => [new("EN"), new("NL")];

	public static CultureInfo GetDefaultCulture() => new("EN");

	private EnvironmentInitialization GetEnvironmentInitialization()
	{
		if (Enum.TryParse<EnvironmentInitialization>(_configuration["Environment:Initialization"], out EnvironmentInitialization env))
			return env;
		return EnvironmentInitialization.None;
	}
}
