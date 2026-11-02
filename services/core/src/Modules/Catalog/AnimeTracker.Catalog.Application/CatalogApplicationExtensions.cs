using AnimeTracker.Catalog.Application.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace AnimeTracker.Catalog.Application;

public static class CatalogApplicationExtensions
{
    public static IServiceCollection AddCatalogApplication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // T3/CA3: Register CatalogOptions with validation on startup
        services
            .AddOptions<CatalogOptions>()
            .Configure<IConfiguration>((opts, cfg) =>
                cfg.GetSection(CatalogOptions.SectionName).Bind(opts))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(CatalogApplicationExtensions).Assembly));
        return services;
    }
}
