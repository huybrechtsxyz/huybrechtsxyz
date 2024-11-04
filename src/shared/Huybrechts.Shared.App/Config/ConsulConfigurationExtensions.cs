using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Consul;
using Serilog;

namespace Huybrechts.Shared.App.Config;

public static class ConsulConfigurationExtensions
{
    /// <summary>
    /// Adds an <see cref="IConfigurationProvider"/> that reads configuration values from Consul.
    /// </summary>
    /// <param name="configurationBuilder">The <see cref="IConfigurationBuilder"/> to add to.</param>
    /// <returns>The <see cref="IConfigurationBuilder"/>.</returns>
    public static IConfigurationBuilder AddConsul(this IConfigurationBuilder configurationBuilder, ILogger logger)
    {
        return configurationBuilder.Add(new ConsulConfigurationSource(logger));
    }

    /// <summary>
    /// Adds an <see cref="IConfigurationProvider"/> that reads configuration values from Consul
    /// with a specified prefix.
    /// </summary>
    /// <param name="configurationBuilder">The <see cref="IConfigurationBuilder"/> to add to.</param>
    /// <param name="prefix">The prefix that Consul keys must start with. The prefix will be removed from the keys.</param>
    /// <returns>The <see cref="IConfigurationBuilder"/>.</returns>
    public static IConfigurationBuilder AddConsul(this IConfigurationBuilder configurationBuilder, string prefix, ILogger logger)
    {
        return configurationBuilder.Add(new ConsulConfigurationSource(logger)
        {
            Prefix = prefix
        });
    }

    /// <summary>
    /// Adds an <see cref="IConfigurationProvider"/> that reads configuration values from Consul
    /// with a specified prefix.
    /// </summary>
    /// <param name="configurationBuilder">The <see cref="IConfigurationBuilder"/> to add to.</param>
    /// <param name="prefix">The prefix that Consul keys must start with. The prefix will be removed from the keys.</param>
    /// <param name="configure">Configure extra options, such as Prefixes and Consul Consistency level</param>
    /// <returns>The <see cref="IConfigurationBuilder"/>.</returns>
    public static IConfigurationBuilder AddConsul(
        this IConfigurationBuilder configurationBuilder, 
        Action<ConsulConfigurationSource> configure,
        ILogger logger)
    {
        var source = new ConsulConfigurationSource(logger);
        configure(source);

        return configurationBuilder.Add(source);
    }
}
