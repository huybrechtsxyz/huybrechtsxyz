using Microsoft.AspNetCore.Hosting;

namespace Huybrechts.App.Extensions;

public static class WebHostEnvironmentExtensions
{
    public static bool IsLocalhost(this IWebHostEnvironment env)
    {
        return env.EnvironmentName == "Localhost";
    }
}
