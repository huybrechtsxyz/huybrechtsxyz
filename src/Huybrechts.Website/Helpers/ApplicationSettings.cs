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

	public string GetApplicationDatabaseConnectionString() => 
		_configuration.GetConnectionString("ApplicationDatabase") ??
		throw new InvalidOperationException("Connection string 'ApplicationDatabase' not found.");
}
