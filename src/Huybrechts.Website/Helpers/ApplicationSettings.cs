using Huybrechts.Website.Models;
using Microsoft.Extensions.Configuration;
using System.Globalization;

namespace Huybrechts.Website.Helpers;

public class ApplicationSettings
{
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

    public enum EnvironmentInitialization
	{
		None = 0,
		Reset = 1,
		Initialize = 2
	}
}
