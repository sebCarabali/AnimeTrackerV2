using Microsoft.Extensions.DependencyInjection;

namespace AnimeTracker.Calendar.Application;

public static class CalendarApplicationExtensions
{
    public static IServiceCollection AddCalendarApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(CalendarApplicationExtensions).Assembly));
        return services;
    }
}
