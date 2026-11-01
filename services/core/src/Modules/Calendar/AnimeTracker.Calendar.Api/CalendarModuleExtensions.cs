using AnimeTracker.Calendar.Application;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AnimeTracker.Calendar.Api;

public static class CalendarModuleExtensions
{
    public static IServiceCollection AddCalendarModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Infrastructure is registered by Host to maintain architectural boundaries.
        // Api only registers Application layer.
        services.AddCalendarApplication(configuration);
        services.AddHealthChecks().AddCheck<CalendarHealthCheck>("calendar");
        return services;
    }

    public static IEndpointRouteBuilder MapCalendarModule(this IEndpointRouteBuilder app)
    {
        return app;
    }
}
