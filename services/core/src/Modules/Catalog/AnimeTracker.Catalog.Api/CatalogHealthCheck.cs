using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace AnimeTracker.Catalog.Api;

internal sealed class CatalogHealthCheck : IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        => Task.FromResult(HealthCheckResult.Healthy("Catalog module is running."));
}
