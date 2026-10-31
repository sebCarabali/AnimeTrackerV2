using Microsoft.Extensions.DependencyInjection;

namespace AnimeTracker.Tracking.Application;

public static class TrackingApplicationExtensions
{
    public static IServiceCollection AddTrackingApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(TrackingApplicationExtensions).Assembly));
        return services;
    }
}
