using System.Globalization;

namespace Huybrechts.Website.Helpers;

public class ApplicationSettings
{
	private readonly IConfiguration _configuration;

	public static CultureInfo[] GetSupportedCultures() => [new CultureInfo("EN"), new CultureInfo("NL")];

	public static CultureInfo GetDefaultCulture() => new CultureInfo("EN");

	public ApplicationSettings(IConfiguration configuration)
	{
		_configuration = configuration;
	}

}
