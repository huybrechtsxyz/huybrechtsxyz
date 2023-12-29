using Huybrechts.Infra.Data;
using Microsoft.Extensions.Configuration;
using System.Globalization;

namespace Huybrechts.Infra.Config;

public class ApplicationSettings
{
	public static readonly string ENV_DOTNET_RUNNING_IN_CONTAINER = "DOTNET_RUNNING_IN_CONTAINER";

    public static bool IsRunningInContainer() => (System.Environment.GetEnvironmentVariable(ENV_DOTNET_RUNNING_IN_CONTAINER) == "true");

    private readonly IConfiguration _configuration;

	public static CultureInfo[] GetSupportedCultures() => [new("EN"), new("NL")];

	public static CultureInfo GetDefaultCulture() => new("EN");

	public ApplicationSettings(IConfiguration configuration)
	{
		_configuration = configuration;
	}

    public bool DoInitializeEnvironment() => GetEnvironmentInitialization() == EnvironmentInitialization.Initialize;

    public bool DoResetEnvironment() => GetEnvironmentInitialization() == EnvironmentInitialization.Reset;

    public string GetAdministrationConnectionString()
    {
        return _configuration.GetConnectionString("AdministrationDatabase") ??
        throw new InvalidOperationException("Connection string 'AdministrationDatabase' not found.");
    }

    public DatabaseProviderType GetAdministrationDatabaseType()
	{
        if (Enum.TryParse<DatabaseProviderType>(_configuration["Environment:AdministrationDatabaseType"], out DatabaseProviderType dbtype))
            return dbtype;
        throw new InvalidCastException("Invalid Environment:AdministrationDatabaseType for type of DatabaseProviderType");
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
		_configuration.GetSection("SeedWorker").Bind(item);
		return item;
	}
		
	public enum EnvironmentInitialization
	{
		None = 0,
		Reset = 1,
		Initialize = 2
	}
}
