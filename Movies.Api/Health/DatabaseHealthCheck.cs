using System.Data;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Movies.Application.Database;

namespace Movies.Api.Health;

public class DatabaseHealthCheck : IHealthCheck
{
    public const string Name = "Database";
    private readonly IDbConnectionFactory _dbConnectionFactory;
    private readonly ILogger<DatabaseHealthCheck> _logger;   

    public DatabaseHealthCheck(IDbConnectionFactory dbConnectionFactory, ILogger<DatabaseHealthCheck> logger)
    {
        _dbConnectionFactory = dbConnectionFactory;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            using var connection = await _dbConnectionFactory.CreateConnectionAsync(cancellationToken);
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT 1";
            await using var reader = await ((dynamic)command).ExecuteReaderAsync(cancellationToken);
            
            return HealthCheckResult.Healthy();
        }
        catch (Exception ex)
        {
            var errorMessage = "Database connectivity check failed";
            _logger.LogError(ex, errorMessage);
            return HealthCheckResult.Unhealthy(errorMessage, ex);
        }
    }
}
