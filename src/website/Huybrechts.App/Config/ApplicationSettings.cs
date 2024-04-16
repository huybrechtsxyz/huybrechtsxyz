using Huybrechts.App.Config.Options;
using Huybrechts.App.Data;
using Microsoft.Extensions.Configuration;
using System.Globalization;
using System.Xml.Linq;

namespace Huybrechts.App.Config;

public sealed class ApplicationSettings
{
	private const string ENV_DOTNET_RUNNING_IN_CONTAINER = "DOTNET_RUNNING_IN_CONTAINER";
	private const string ENV_APP_DATA_TYPE = "APP_DATA_TYPE";
	private const string ENV_APP_DATA_URL = "APP_DATA_URL";
	private const string ENV_APP_DATA_NAME = "APP_DATA_NAME";
	private const string ENV_APP_DATA_USERNAME = "APP_DATA_USERNAME";
	private const string ENV_APP_DATA_PASSWORD = "APP_DATA_PASSWORD";
	private const string OPT_APP_DATA_URL = "DatabaseContext";

	public static CultureInfo[] GetSupportedCultures() => [new("EN"), new("NL")];

	public static CultureInfo GetDefaultCulture() => new("EN");

	public static bool IsRunningInContainer() => Environment.GetEnvironmentVariable(ENV_DOTNET_RUNNING_IN_CONTAINER) == "true";

	public static string GetApplicationConnectionString(IConfiguration configuration)
	{
		string? dataUrl = Environment.GetEnvironmentVariable(ENV_APP_DATA_URL);
		if (string.IsNullOrEmpty(dataUrl))
			dataUrl = configuration.GetConnectionString(OPT_APP_DATA_URL);
		if (dataUrl is null)
			throw new InvalidOperationException($"Connection string for {OPT_APP_DATA_URL} not found.");

		if (dataUrl.Contains("{database}", StringComparison.CurrentCultureIgnoreCase))
		{
			var dbname = configuration.GetValue<string>(ENV_APP_DATA_NAME) ?? string.Empty;
			if (dbname.EndsWith("_FILE", StringComparison.InvariantCultureIgnoreCase)
				|| dbname.StartsWith("/run/secrets", StringComparison.CurrentCultureIgnoreCase))
				dbname = File.ReadAllText(dbname);
			dataUrl = dataUrl.Replace("{database}", dbname, StringComparison.InvariantCultureIgnoreCase);
		}

		if (dataUrl.Contains("{user}", StringComparison.CurrentCultureIgnoreCase))
		{
			var user = configuration.GetValue<string>(ENV_APP_DATA_USERNAME) ?? string.Empty;
			if (user.EndsWith("_FILE", StringComparison.InvariantCultureIgnoreCase)
				|| user.StartsWith("/run/secrets", StringComparison.CurrentCultureIgnoreCase))
				user = File.ReadAllText(user);
			dataUrl = dataUrl.Replace("{user}", user, StringComparison.InvariantCultureIgnoreCase);
		}

		if (dataUrl.Contains("{password}", StringComparison.CurrentCultureIgnoreCase))
		{
			var pass = configuration.GetValue<string>(ENV_APP_DATA_PASSWORD) ?? string.Empty;
			if (pass.EndsWith("_FILE", StringComparison.InvariantCultureIgnoreCase)
				|| pass.StartsWith("/run/secrets", StringComparison.CurrentCultureIgnoreCase))
				pass = File.ReadAllText(pass);
			dataUrl = dataUrl.Replace("{password}", pass, StringComparison.InvariantCultureIgnoreCase);
		}

		return dataUrl;
	}

	/// <summary>
	/// Check the configuration for the APP_DATA_TYPE (commandline or envvar)
	/// Otherwise check the configured environment option
	/// </summary>
	/// <param name="configuration">IConfiguration</param>
	/// <param name="environment">EnvironmentOptions</param>
	/// <returns></returns>
	/// <exception cref="InvalidOperationException"></exception>
	public static DatabaseProviderType GetApplicationConnectionType(IConfiguration configuration, EnvironmentOptions environment)
	{
		string? provider = configuration.GetValue<string>(ENV_APP_DATA_TYPE);
		if (!string.IsNullOrEmpty(provider))
		{
			if (Enum.TryParse<DatabaseProviderType>(provider, out DatabaseProviderType dbtype))
				return dbtype;
		}

		if (environment.DatabaseProviderType != DatabaseProviderType.None)
			return environment.DatabaseProviderType;

		throw new InvalidOperationException($"Invalid DatabaseProviderType for {provider}");
	}
}
