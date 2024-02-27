using Microsoft.AspNetCore.Hosting;

namespace Huybrechts.App.Extensions;

public static class WebHostEnvironmentExtensions
{
    public static bool IsTest(this IWebHostEnvironment env)
    {
        return env.EnvironmentName == "Test";
    }
}
