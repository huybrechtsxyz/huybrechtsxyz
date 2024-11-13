using Huybrechts.Shared.App.Config;
using Serilog;
using Serilog.Extensions.Hosting;

namespace Huybrechts.Shared.Web;

public static class SharedHostingExtensions
{
    public static ReloadableLogger CreateBootstrapLogger(string filename)
    {
        var logfile = (ApplicationSettings.IsRunningInContainer() ? $"/app/logs/{filename}.log" : $"/.app/logs/{filename}.log");
        return new LoggerConfiguration()
            .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Information)
            .Enrich.FromLogContext()
            .WriteTo.File(logfile, rollingInterval: RollingInterval.Day, retainedFileCountLimit: 7) // Log to file
            .CreateBootstrapLogger();
    }
}
