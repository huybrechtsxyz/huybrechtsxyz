using Huybrechts.App.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Protocols.Configuration;
using System.Globalization;

namespace Huybrechts.App.Config;

public sealed class ApplicationSettings
{
	private const string ENV_DOTNET_RUNNING_IN_CONTAINER = "DOTNET_RUNNING_IN_CONTAINER";
	private const string ENV_APP_DATA_URL = "APP_DATA_URL";
	private const string ENV_APP_DATA_NAME = "APP_DATA_NAME";
	private const string ENV_APP_DATA_USERNAME = "APP_DATA_USERNAME";
	private const string ENV_APP_DATA_PASSWORD = "APP_DATA_PASSWORD";

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
			dataUrl = dataUrl.Replace("{database}", dbname, StringComparison.InvariantCultureIgnoreCase);
		}

		if (dataUrl.Contains("{username}", StringComparison.InvariantCultureIgnoreCase))
		{
			var username = configuration.GetValue<string>(ENV_APP_DATA_USERNAME) ?? string.Empty;
			if (username.EndsWith("_FILE", StringComparison.InvariantCultureIgnoreCase)
				|| username.StartsWith("/run/secrets", StringComparison.CurrentCultureIgnoreCase))
				username = File.ReadAllText(username);
			dataUrl = dataUrl.Replace("{username}", username, StringComparison.InvariantCultureIgnoreCase);
		}

		if (dataUrl.Contains("{password}", StringComparison.InvariantCultureIgnoreCase))
		{
			var pass = configuration.GetValue<string>(ENV_APP_DATA_PASSWORD) ?? string.Empty;
			if (pass.EndsWith("_FILE", StringComparison.InvariantCultureIgnoreCase)
				|| pass.StartsWith("/run/secrets", StringComparison.CurrentCultureIgnoreCase))
				pass = File.ReadAllText(pass);
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
}
