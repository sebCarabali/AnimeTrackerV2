using AnimeTracker.Calendar.Application.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace AnimeTracker.Calendar.Application;

public static class CalendarApplicationExtensions
{
    public static IServiceCollection AddCalendarApplication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // T3/CA3: Register CalendarOptions with validation on startup
        services
            .AddOptions<CalendarOptions>()
            .Configure<IConfiguration>((opts, cfg) =>
                cfg.GetSection(CalendarOptions.SectionName).Bind(opts))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(CalendarApplicationExtensions).Assembly));
        return services;
    }
}
