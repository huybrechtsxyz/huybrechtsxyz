﻿using Huybrechts.App.Config.Options;
using Huybrechts.App.Data;
using Huybrechts.App.Identity.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Protocols.Configuration;
using System.Globalization;

namespace Huybrechts.App.Config;

public sealed class ApplicationSettings
{
	private const string ENV_DOTNET_RUNNING_IN_CONTAINER = "DOTNET_RUNNING_IN_CONTAINER";
	private const string ENV_CONTEXT = "CONTEXT";
	private const string ENV_APP_DATA_URL = "APP_DATA_URL";
	private const string ENV_APP_DATA_NAME = "APP_DATA_NAME";
	private const string ENV_APP_DATA_USERNAME = "APP_DATA_USERNAME";
	private const string ENV_APP_DATA_PASSWORD = "APP_DATA_PASSWORD";

	public const string ENV_APP_HOST_EMAIL = "APP_HOST_EMAIL";
	public const string ENV_APP_HOST_USERNAME = "APP_HOST_USERNAME";
	public const string ENV_APP_HOST_PASSWORD = "APP_HOST_PASSWORD";
	public const string ENV_APP_AUTH_GOOGLE = "APP_AUTH_GOOGLE";
	public const string ENV_APP_SMTP_OPTIONS = "APP_SMTP_OPTIONS";

	public static CultureInfo[] GetSupportedCultures() => [new("EN"), new("NL")];

	public static CultureInfo GetDefaultCulture() => new("EN");

	public static bool IsRunningInContainer() => Environment.GetEnvironmentVariable(ENV_DOTNET_RUNNING_IN_CONTAINER) == "true";

	public static string GetApplicationContextConnectionString(IConfiguration configuration)
	{
		var context = (configuration.GetValue<string>(ENV_CONTEXT) ?? string.Empty) + nameof(ApplicationContext);

        var dataUrl = configuration.GetValue<string>(ENV_APP_DATA_URL) ?? string.Empty;
		if (string.IsNullOrWhiteSpace(dataUrl))
			dataUrl = configuration.GetConnectionString(context);
		if (dataUrl is null)
			throw new InvalidConfigurationException($"Connection string for '{context}' not found.");

		if (dataUrl.Contains("{database}", StringComparison.InvariantCultureIgnoreCase))
		{
			var database = configuration.GetValue<string>(ENV_APP_DATA_NAME) ?? string.Empty;
			if (!string.IsNullOrEmpty(database))
				dataUrl = dataUrl.Replace("{database}", database, StringComparison.InvariantCultureIgnoreCase);
		}

		if (dataUrl.Contains("{username}", StringComparison.InvariantCultureIgnoreCase))
		{
			var username = configuration.GetValue<string>(ENV_APP_DATA_USERNAME) ?? string.Empty;
			if (!string.IsNullOrEmpty(username))
				dataUrl = dataUrl.Replace("{username}", username, StringComparison.InvariantCultureIgnoreCase);
		}

		if (dataUrl.Contains("{password}", StringComparison.InvariantCultureIgnoreCase))
		{
			var password = configuration.GetValue<string>(ENV_APP_DATA_PASSWORD) ?? string.Empty;
			if (!string.IsNullOrEmpty(password)) 
				dataUrl = dataUrl.Replace("{password}", password, StringComparison.InvariantCultureIgnoreCase);
		}

		return dataUrl;
	}

	public static ContextProviderType GetContextProvider(string? connectionString)
	{
		if (string.IsNullOrWhiteSpace(connectionString))
			return ContextProviderType.None;

		// SQLite connection string pattern
		else if (connectionString.Contains("Data Source=", StringComparison.InvariantCultureIgnoreCase))
			return ContextProviderType.Sqlite;

		// SQL Server connection string pattern
		else if (connectionString.Contains("Server="), StringComparison.InvariantCultureIgnoreCase)
			return ContextProviderType.SqlServer;

		// PostgreSQL connection string pattern
		//else if (connectionString.Contains("Host=") && connectionString.Contains("Port="))
		//	return ContextProviderType.Postgres;

		// Unable to determine database provider
		else
			throw new ArgumentException("Unsupported or invalid connection string format.");
	}

	public static GoogleLoginOptions GetGoogleLoginOptions(IConfiguration configuration)
	{
		GoogleLoginOptions options = new ();
		configuration.GetSection(ENV_APP_AUTH_GOOGLE).Bind(options);
		return options;
	}
}
