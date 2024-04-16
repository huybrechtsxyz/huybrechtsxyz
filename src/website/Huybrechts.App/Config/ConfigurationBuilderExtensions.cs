using Microsoft.Extensions.Configuration;

namespace Huybrechts.App.Config;

public static class ConfigurationBuilderExtensions
{
	public static IConfigurationBuilder AddDockerSecrets(
			this IConfigurationBuilder builder,
			string secretsDirectoryPath,
			string colonPlaceholder,
			ICollection<string>? allowedPrefixes)
	{
		return builder.Add(new DockerSecretsConfigurationsSource(
			secretsDirectoryPath,
			colonPlaceholder,
			allowedPrefixes));
	}
}
