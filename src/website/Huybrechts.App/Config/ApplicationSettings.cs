using Huybrechts.App.Data;
using Microsoft.Extensions.Configuration;
using System.Globalization;

namespace Huybrechts.App.Config;

public sealed class ApplicationSettings
{
	private readonly IConfiguration _configuration;
	private readonly CommandLineOptions? _options;

	public static readonly string ENV_DOTNET_RUNNING_IN_CONTAINER = "DOTNET_RUNNING_IN_CONTAINER";
	public static readonly string ENV_APP_DATA_TYPE = "APP_DATA_TYPE";
	public static readonly string ENV_APP_DATA_URL = "APP_DATA_URL";
	public static readonly string ENV_APP_DATA_NAME = "APP_DATA_NAME";

	public static bool IsRunningInContainer() => (System.Environment.GetEnvironmentVariable(ENV_DOTNET_RUNNING_IN_CONTAINER) == "true");

	public ApplicationSettings(IConfiguration configuration, CommandLineOptions? options = default)
	{
		_configuration = configuration;
		_options = options;
	}

	public bool DoInitializeEnvironment() => GetEnvironmentInitialization() == EnvironmentInitialization.Initialize;

	public bool DoResetEnvironment() => GetEnvironmentInitialization() == EnvironmentInitialization.Reset;

	public string GetApplicationConnectionString()
	{
		string? dataUrl = Environment.GetEnvironmentVariable(ENV_APP_DATA_URL);
		if (string.IsNullOrEmpty(dataUrl))
		{
			if (_options is not null && !string.IsNullOrEmpty(_options.ConnectionString))
				dataUrl = _options.ConnectionString;
			else
				dataUrl = _configuration.GetConnectionString("DatabaseContext");
		}
		if (dataUrl is null)
			throw new InvalidOperationException("Connection string for DatabaseContext not found.");

		var dbname = Environment.GetEnvironmentVariable(ENV_APP_DATA_NAME);
		if (!string.IsNullOrEmpty(dbname) && dataUrl.Contains("{database}"))
		{
			if (dbname.ToUpper().EndsWith("_FILE") || dbname.StartsWith("/run/secrets", StringComparison.CurrentCultureIgnoreCase))
				dbname = File.ReadAllText(dbname);
			dataUrl = dataUrl.Replace("{database}", dbname);
		}

		var user = Environment.GetEnvironmentVariable(_configuration["Environment:Username"] ?? string.Empty);
		if (!string.IsNullOrEmpty(user) && dataUrl.Contains("{username}"))
		{
			if (user.ToUpper().EndsWith("_FILE") || user.StartsWith("/run/secrets", StringComparison.CurrentCultureIgnoreCase))
				user = File.ReadAllText(user);
			dataUrl = dataUrl.Replace("{username}", user);
		}

		var pass = Environment.GetEnvironmentVariable(_configuration["Environment:Password"] ?? string.Empty);
		if (!string.IsNullOrEmpty(pass) && dataUrl.Contains("{password}"))
		{
			if (pass.ToUpper().EndsWith("_FILE") || pass.StartsWith("/run/secrets", StringComparison.CurrentCultureIgnoreCase))
				pass = File.ReadAllText(pass);
			dataUrl = dataUrl.Replace("{password}", pass);
		}

		return dataUrl;
	}

	public DatabaseProviderType GetApplicationConnectionType()
	{
		string? provider = Environment.GetEnvironmentVariable(ENV_APP_DATA_TYPE);
		if (string.IsNullOrEmpty(provider))
		{
			if (_options is not null && _options.DatabaseProviderType != DatabaseProviderType.None)
				return _options.DatabaseProviderType;

			provider = _configuration["Environment:DatabaseType"];
			if (string.IsNullOrEmpty(provider))
				throw new InvalidOperationException("DatabaseProviderType not found.");
		}
		
		if (Enum.TryParse<DatabaseProviderType>(provider, out DatabaseProviderType dbtype))
			return dbtype;

		throw new InvalidCastException($"Invalid DatabaseProviderType for {provider}");
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
