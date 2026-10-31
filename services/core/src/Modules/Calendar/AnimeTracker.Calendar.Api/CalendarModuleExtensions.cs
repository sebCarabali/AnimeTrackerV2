using AnimeTracker.Calendar.Application;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace AnimeTracker.Calendar.Api;

public static class CalendarModuleExtensions
{
    public static IServiceCollection AddCalendarModule(this IServiceCollection services)
    {
        services.AddCalendarApplication();
        services.AddHealthChecks().AddCheck<CalendarHealthCheck>("calendar");
        return services;
    }

    public static IEndpointRouteBuilder MapCalendarModule(this IEndpointRouteBuilder app)
    {
        return app;
    }
}
