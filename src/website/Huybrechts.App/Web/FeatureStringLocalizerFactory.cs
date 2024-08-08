using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Serilog.Core;
using System.Reflection;
using System.Resources;

namespace Huybrechts.App.Web;

public class FeatureStringLocalizerFactory : IStringLocalizerFactory
{
    private readonly IWebHostEnvironment _env;
    private readonly IResourceNamesCache _cache;
    private readonly ILoggerFactory _logger;

    public FeatureStringLocalizerFactory(IWebHostEnvironment env, IResourceNamesCache resourceNamesCache, ILoggerFactory logger)
    {
        _env = env;
        _cache = resourceNamesCache;
        _logger = logger;
    }

    public IStringLocalizer Create(Type resourceSource) => CreateLocalizer(resourceSource.FullName ?? resourceSource.Name);

    public IStringLocalizer Create(string baseName, string location)
    {
        return CreateLocalizer(baseName);
    }

    private ResourceManagerStringLocalizer CreateLocalizer(string baseName)
    {
        ArgumentException.ThrowIfNullOrEmpty(baseName, nameof(baseName));

        var baseNamespace = baseName.Substring(0, baseName.LastIndexOf('.'));
        var featureName = baseNamespace.Split('.').Last();

        var resourcePath = Path.Combine(_env.ContentRootPath, "Resources", $"{baseName}.resx");
        if (!File.Exists(resourcePath))
            resourcePath = Path.Combine(_env.ContentRootPath, "Features", featureName, "Localization.resx");

        var resourceManager = new ResourceManager(baseName, Assembly.GetExecutingAssembly());
        var log = _logger.CreateLogger<ResourceManagerStringLocalizer>();

        return new ResourceManagerStringLocalizer(resourceManager, Assembly.GetExecutingAssembly(), baseName, _cache, log);
    }
}
