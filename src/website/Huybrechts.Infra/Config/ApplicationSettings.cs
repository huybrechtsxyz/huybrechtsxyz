using Huybrechts.Infra.Data;
using Microsoft.Extensions.Configuration;
using System.Globalization;

namespace Huybrechts.Infra.Config;

public sealed class ApplicationSettings
{
	private readonly IConfiguration _configuration;

	public static readonly string ENV_DOTNET_RUNNING_IN_CONTAINER = "DOTNET_RUNNING_IN_CONTAINER";

    public static bool IsRunningInContainer() => (System.Environment.GetEnvironmentVariable(ENV_DOTNET_RUNNING_IN_CONTAINER) == "true");

	public ApplicationSettings(IConfiguration configuration)
	{
		_configuration = configuration;
	}

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

	public static CultureInfo[] GetSupportedCultures() => [new("EN"), new("NL")];

	public static CultureInfo GetDefaultCulture() => new("EN");
}
