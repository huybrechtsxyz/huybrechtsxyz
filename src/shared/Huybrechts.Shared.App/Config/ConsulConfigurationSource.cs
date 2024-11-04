using Consul;
using Serilog;

namespace Microsoft.Extensions.Configuration.Consul
{
    public class ConsulConfigurationSource : IConfigurationSource
    {
        private readonly ILogger _logger;

        public string Prefix { get; set; }

        public QueryOptions Options { get; set; }

        public ConsulConfigurationSource(ILogger logger)
        {
            Prefix = string.Empty;
            Options = QueryOptions.Default;
            _logger = logger;
        }

        public IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            return new ConsulConfigurationProvider(() => new ConsulClient(), Options, Prefix, _logger);
        }
    }
}