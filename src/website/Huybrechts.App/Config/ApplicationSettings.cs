using Humanizer.Configuration;
using Huybrechts.App.Config.Options;
using Huybrechts.App.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Protocols.Configuration;
using System.Globalization;
using System.Xml.Linq;

namespace Huybrechts.App.Config;

public sealed class ApplicationSettings
{
	private const string ENV_DOTNET_RUNNING_IN_CONTAINER = "DOTNET_RUNNING_IN_CONTAINER";
	private const string ENV_APP_DATA_URL = "APP_DATA_URL";
	private const string ENV_APP_DATA_NAME = "APP_DATA_NAME";
	private const string ENV_APP_DATA_USERNAME = "APP_DATA_USERNAME";
	private const string ENV_APP_DATA_PASSWORD = "APP_DATA_PASSWORD";
	private const string ENV_APP_SMTP_OPTIONS = "APP_SMTP_OPTIONS";

	public static CultureInfo[] GetSupportedCultures() => [new("EN"), new("NL")];

	public static CultureInfo GetDefaultCulture() => new("EN");

	public static bool IsRunningInContainer() => Environment.GetEnvironmentVariable(ENV_DOTNET_RUNNING_IN_CONTAINER) == "true";

	public static string GetApplicationContextConnectionString(IConfiguration configuration)
	{
		var dataUrl = configuration.GetValue<string>(ENV_APP_DATA_URL);
		if (string.IsNullOrWhiteSpace(dataUrl))
			dataUrl = configuration.GetConnectionString(nameof(ApplicationContext));
		if (dataUrl is null)
			throw new InvalidConfigurationException($"Connection string for {nameof(ApplicationContext)} not found.");

		if (dataUrl.Contains("{database}", StringComparison.InvariantCultureIgnoreCase))
		{
			var dbname = configuration.GetValue<string>(ENV_APP_DATA_NAME) ?? string.Empty;
			if (dbname.EndsWith("_FILE", StringComparison.InvariantCultureIgnoreCase)
				|| dbname.StartsWith("/run/secrets", StringComparison.CurrentCultureIgnoreCase))
				dbname = File.ReadAllText(dbname);
			if (!string.IsNullOrEmpty(dbname))
				dataUrl = dataUrl.Replace("{database}", dbname, StringComparison.InvariantCultureIgnoreCase);
		}

		if (dataUrl.Contains("{username}", StringComparison.InvariantCultureIgnoreCase))
		{
			var username = configuration.GetValue<string>(ENV_APP_DATA_USERNAME) ?? string.Empty;
			if (username.EndsWith("_FILE", StringComparison.InvariantCultureIgnoreCase)
				|| username.StartsWith("/run/secrets", StringComparison.CurrentCultureIgnoreCase))
				username = File.ReadAllText(username);
			if (!string.IsNullOrEmpty(username))
				dataUrl = dataUrl.Replace("{username}", username, StringComparison.InvariantCultureIgnoreCase);
		}

		if (dataUrl.Contains("{password}", StringComparison.InvariantCultureIgnoreCase))
		{
			var pass = configuration.GetValue<string>(ENV_APP_DATA_PASSWORD) ?? string.Empty;
			if (pass.EndsWith("_FILE", StringComparison.InvariantCultureIgnoreCase)
				|| pass.StartsWith("/run/secrets", StringComparison.CurrentCultureIgnoreCase))
				pass = File.ReadAllText(pass);
			if (!string.IsNullOrEmpty(pass)) 
				dataUrl = dataUrl.Replace("{password}", pass, StringComparison.InvariantCultureIgnoreCase);
		}

		return dataUrl;
	}

	public static ContextProviderType GetContextProvider(string? connectionString)
	{
		if (string.IsNullOrWhiteSpace(connectionString))
			return ContextProviderType.None;

		// SQLite connection string pattern
		else if (connectionString.Contains("Data Source="))
			return ContextProviderType.Sqlite;

		// SQL Server connection string pattern
		//else if (connectionString.Contains("Server=") || connectionString.Contains("Data Source="))
		//	return ContextProviderType.SqlServer;

		// PostgreSQL connection string pattern
		//else if (connectionString.Contains("Host=") && connectionString.Contains("Port="))
		//	return ContextProviderType.Postgres;

		// Unable to determine database provider
		else
			throw new ArgumentException("Unsupported or invalid connection string format.");
	}

	public static void AddSmtpServerOptions(WebApplicationBuilder builder)
	{
		SmtpServerOptions item = new();
		builder.Configuration.GetSection(nameof(SmtpServerOptions)).Bind(item);

		SmtpServerOptions? secret = null;
		var value = builder.Configuration.GetValue<string>(ENV_APP_SMTP_OPTIONS) ?? string.Empty;
		if (value.EndsWith("_FILE", StringComparison.InvariantCultureIgnoreCase)
			|| value.StartsWith("/run/secrets", StringComparison.CurrentCultureIgnoreCase))
			value = File.ReadAllText(value);

		if (!string.IsNullOrEmpty(value))
			secret = System.Text.Json.JsonSerializer.Deserialize<SmtpServerOptions>(value);

		if (secret is not null)
			builder.Services.AddSingleton(secret);
		else
			builder.Services.AddSingleton(item);
	}

	private static string GetConfigurationValue(IConfiguration configuration, string key)
	{
		var value = configuration.GetValue<string>(key) ?? string.Empty;
		if (string.IsNullOrEmpty(value) ||
			(!value.EndsWith("_FILE", StringComparison.InvariantCultureIgnoreCase)
			&& !value.StartsWith("/run/secrets", StringComparison.CurrentCultureIgnoreCase)))
			return value;

		value = File.ReadAllText(value);
		return value;
	}
}
