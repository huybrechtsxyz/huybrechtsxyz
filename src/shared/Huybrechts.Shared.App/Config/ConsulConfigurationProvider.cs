using Consul;
using Microsoft.Extensions.Configuration;
using System.Text;
using Serilog;

namespace Huybrechts.Shared.App.Config;

public class ConsulConfigurationProvider : ConfigurationProvider
{
    private readonly ILogger _logger;
    private readonly Func<IConsulClient> _clientFactory;
    private readonly QueryOptions _options;
    private readonly string _prefix;

    public ConsulConfigurationProvider(Func<IConsulClient> clientFactory, QueryOptions options, string prefix, ILogger logger)
    {
        _clientFactory = clientFactory;
        _options = options;
        _prefix = prefix ?? string.Empty;
        _logger = logger;
    }

    public override void Load()
    {
        try
        {
            Data = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);

            using var client = _clientFactory();
            var results = client.KV
                .List(_prefix, _options)
                .Result // ehhh, we have no async version of Load :(
                .Response;

            if (results == null)
                return;

            foreach (var pair in results)
            {
                var key = ReplacePathDelimiters(RemovePrefix(pair.Key));
                var value = AsString(pair.Value);

                Data[key] = value;
            }
        }
        catch (Exception ex)
        {
            _logger.Warning(ex, "Unable to load configuration from CONSUL");
        }
    }

    private static string ReplacePathDelimiters(string input) => input.Replace("/", ConfigurationPath.KeyDelimiter);

    private static string AsString(byte[] bytes) => Encoding.UTF8.GetString(bytes, 0, bytes.Length);

    private string RemovePrefix(string input) => input[_prefix.Length..];
}