using AnimeTracker.Tracking.Application.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace AnimeTracker.Tracking.Application;

public static class TrackingApplicationExtensions
{
    public static IServiceCollection AddTrackingApplication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // T3/CA3: Register TrackingOptions with validation on startup
        services
            .AddOptions<TrackingOptions>()
            .Configure<IConfiguration>((opts, cfg) =>
                cfg.GetSection(TrackingOptions.SectionName).Bind(opts))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(TrackingApplicationExtensions).Assembly));
        return services;
    }
}
