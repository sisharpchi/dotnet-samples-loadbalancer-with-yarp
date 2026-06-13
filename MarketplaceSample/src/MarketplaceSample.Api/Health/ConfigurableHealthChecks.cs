using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace MarketplaceSample.Api.Health;

public sealed class ConfigurableLivenessHealthCheck(IConfiguration configuration) : IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(IsEnabled(configuration, "Health:Live")
            ? HealthCheckResult.Healthy("Instance is live.")
            : HealthCheckResult.Unhealthy("Instance liveness was disabled by configuration."));
    }

    private static bool IsEnabled(IConfiguration configuration, string key)
    {
        return configuration.GetValue(key, true);
    }
}

public sealed class ConfigurableReadinessHealthCheck(IConfiguration configuration) : IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(configuration.GetValue("Health:Ready", true)
            ? HealthCheckResult.Healthy("Instance is ready.")
            : HealthCheckResult.Unhealthy("Instance readiness was disabled by configuration."));
    }
}
