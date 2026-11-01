using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace AnimeTracker.Tracking.Api;

internal sealed class TrackingHealthCheck : IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        => Task.FromResult(HealthCheckResult.Healthy("Tracking module is running."));
}
