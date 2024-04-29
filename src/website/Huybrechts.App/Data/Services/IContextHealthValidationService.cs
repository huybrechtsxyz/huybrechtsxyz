using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Huybrechts.App.Data.Services;

public interface IContextHealthValidationService
{
    HealthStatus GetHealthStatus(string connectionstring, int maxRetries, int initialDelaySeconds);
}
