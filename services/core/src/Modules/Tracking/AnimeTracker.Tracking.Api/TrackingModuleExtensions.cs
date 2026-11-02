using AnimeTracker.Tracking.Application;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AnimeTracker.Tracking.Api;

public static class TrackingModuleExtensions
{
    public static IServiceCollection AddTrackingModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Infrastructure is registered by Host to maintain architectural boundaries.
        // Api only registers Application layer.
        services.AddTrackingApplication(configuration);
        services.AddHealthChecks().AddCheck<TrackingHealthCheck>("tracking");
        return services;
    }

    public static IEndpointRouteBuilder MapTrackingModule(this IEndpointRouteBuilder app)
    {
        return app;
    }
}
