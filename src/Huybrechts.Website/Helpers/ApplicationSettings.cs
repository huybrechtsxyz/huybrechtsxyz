using Huybrechts.Website.Data;
using Huybrechts.Website.Models;
using Microsoft.Extensions.Configuration;
using System.Globalization;

namespace Huybrechts.Website.Helpers;

public class ApplicationSettings
{
	public static readonly string ENV_DOTNET_RUNNING_IN_CONTAINER = "DOTNET_RUNNING_IN_CONTAINER";

	private readonly IConfiguration _configuration;

	public static CultureInfo[] GetSupportedCultures() => [new("EN"), new("NL")];

	public static CultureInfo GetDefaultCulture() => new("EN");

	public ApplicationSettings(IConfiguration configuration)
	{
		_configuration = configuration;
	}

    public bool DoInitializeEnvironment() => GetEnvironmentInitialization() == EnvironmentInitialization.Initialize;

    public bool DoResetEnvironment() => GetEnvironmentInitialization() == EnvironmentInitialization.Reset;
	 
    public string GetApplicationDatabaseConnectionString() => 
		_configuration.GetConnectionString("ApplicationDatabase") ??
		throw new InvalidOperationException("Connection string 'ApplicationDatabase' not found.");

	public DatabaseContextType GetApplicationDatabaseType()
	{
		var type = _configuration.GetValue<DatabaseContextType>("Environment:ApplicationDatabaseType");
		return type;
	}

    public AuthenticationSettings GetGoogleAuthentication()
    {
        AuthenticationSettings item = new();
        _configuration.GetSection("Authentication:Google").Bind(item);
        return item;
    }

    public MessageAuthenticationSettings GetMessageAuthentication()
	{
        MessageAuthenticationSettings item = new();
        _configuration.GetSection("Authentication:Mail").Bind(item);
        return item;
    }

    public MessageServerSettings GetMessageServer()
    {
        MessageServerSettings item = new();
        _configuration.GetSection("Messaging:Mail").Bind(item);
        return item;
    }

    private EnvironmentInitialization GetEnvironmentInitialization()
	{
        if (Enum.TryParse<EnvironmentInitialization>(_configuration["Environment:Initialization"], out EnvironmentInitialization env))
            return env;
        return EnvironmentInitialization.None;
    }

	public SeedWorkerSettings GetSeedWorker()
	{
		SeedWorkerSettings item = new();
		_configuration.GetSection("SeedWorker").Bind(item);
		return item;
	}

	public SeedWorkerSecrets GetSeedSecrets()
	{
		SeedWorkerSecrets item = new();
		_configuration.GetSection("Messaging:Mail").Bind(item);
		return item;
	}

	public bool IsRunningInContainer() => (System.Environment.GetEnvironmentVariable(ENV_DOTNET_RUNNING_IN_CONTAINER) == "true");

	public enum EnvironmentInitialization
	{
		None = 0,
		Reset = 1,
		Initialize = 2
	}
}
