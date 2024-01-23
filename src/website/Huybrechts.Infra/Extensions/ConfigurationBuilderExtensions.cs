using Consul;
using Huybrechts.Infra.Config;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Winton.Extensions.Configuration.Consul;

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

	/// <summary>
	/// public ValuesController(IConsulClient consulClient)  
	/// var res = await _consulClient.KV.Get(key);
	/// 
	/// public class DemoAppOptions
	/// services.Configure<DemoAppOptions>(Configuration.GetSection("DemoAppOptions"));
	/// public ValuesController(IOptions<DemoAppSettings> options)
	/// </summary>
	/// <param name="services"></param>
	/// <param name="configuration"></param>
	/// <returns></returns>
	public static IServiceCollection AddConsulConfig(this IServiceCollection services, IConfiguration configuration)
	{
		var address = configuration.GetConnectionString("ConsulHost");
		if (string.IsNullOrEmpty(address))
			return services;

		

		services.AddSingleton<IConsulClient, ConsulClient>(p => new ConsulClient(consulConfig => { consulConfig.Address = new Uri(address); }
		, null, handlerOverride =>
		{
			//disable proxy of httpclienthandler  
			handlerOverride.Proxy = null;
			handlerOverride.UseProxy = false;
		}));
		return services;
	}

	public static IConfigurationBuilder AddConsulBuilder(this IConfigurationBuilder builder, IConfiguration configuration)
	{
		var address = configuration.GetConnectionString("ConsulHost");
		if (string.IsNullOrEmpty(address))
			return builder;

		builder.AddConsul("Website");

		services.AddSingleton<IConsulClient, ConsulClient>(p => new ConsulClient(consulConfig => { consulConfig.Address = new Uri(address); }
		, null, handlerOverride =>
		{
			//disable proxy of httpclienthandler  
			handlerOverride.Proxy = null;
			handlerOverride.UseProxy = false;
		}));
		return services;
	}
}
