using Huybrechts.App.Config;
using Microsoft.Extensions.Configuration;

namespace Huybrechts.App.Extensions;

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
