using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace AnimeTracker.Calendar.Api;

internal sealed class CalendarHealthCheck : IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        => Task.FromResult(HealthCheckResult.Healthy("Calendar module is running."));
}
