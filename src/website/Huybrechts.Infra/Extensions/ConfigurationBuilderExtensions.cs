using Consul;
using Huybrechts.Infra.Config;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Huybrechts.Infra.Extensions;

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

	public static IServiceCollection AddConsulConfig(this IServiceCollection services, string configKey)
	{
		ArgumentNullException.ThrowIfNull(services);
		services.AddSingleton<IConsulClient>(consul => new ConsulClient(consulConfig =>
		{
			consulConfig.Address = new Uri(configKey);
		}));
		return services;
	}
}
