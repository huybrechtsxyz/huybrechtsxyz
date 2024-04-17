using Huybrechts.App.Data;
using System.Globalization;

namespace Huybrechts.App.Config;

public sealed class ApplicationSettings
{
	private const string ENV_DOTNET_RUNNING_IN_CONTAINER = "DOTNET_RUNNING_IN_CONTAINER";

	public static CultureInfo[] GetSupportedCultures() => [new("EN"), new("NL")];

	public static CultureInfo GetDefaultCulture() => new("EN");

	public static bool IsRunningInContainer() => Environment.GetEnvironmentVariable(ENV_DOTNET_RUNNING_IN_CONTAINER) == "true";

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
