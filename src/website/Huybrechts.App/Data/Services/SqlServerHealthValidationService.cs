using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Huybrechts.App.Data.Services;

public class SqlServerHealthValidationService : IContextHealthValidationService
{
    public HealthStatus GetHealthStatus(string connectionstring, int maxRetries, int initialDelaySeconds)
    {
        SqlConnection? connection = null;
        HealthStatus healthStatus = HealthStatus.Unhealthy;
        bool connected = false;
        int retryCount = 0;

        while (!connected && retryCount < maxRetries)
        {
            try
            {
                connection = new SqlConnection(connectionstring);
                connection.Open();
                // Connection successful
                connected = true;
                healthStatus = HealthStatus.Healthy;
            }
            catch (SqlException ex)
            {
                retryCount++;
                if (retryCount < maxRetries)
                {
                    // Calculate exponential backoff delay
                    int delay = (int)(initialDelaySeconds * 1000 * Math.Pow(2, retryCount));
                    Thread.Sleep(delay);
                }
            }
        }

        if (!connected)
        {
            //throw new Exception($"Failed to connect to SQLite database after {MaxRetries} retries.");
            healthStatus = HealthStatus.Unhealthy;
        }

        return healthStatus;
    }
}
