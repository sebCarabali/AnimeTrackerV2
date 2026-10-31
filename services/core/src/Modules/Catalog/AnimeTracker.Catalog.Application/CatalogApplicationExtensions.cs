using Microsoft.Extensions.DependencyInjection;

namespace AnimeTracker.Catalog.Application;

public static class CatalogApplicationExtensions
{
    public static IServiceCollection AddCatalogApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(CatalogApplicationExtensions).Assembly));
        return services;
    }
}
