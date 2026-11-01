using AnimeTracker.Catalog.Application;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AnimeTracker.Catalog.Api;

public static class CatalogModuleExtensions
{
    public static IServiceCollection AddCatalogModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Infrastructure is registered by Host to maintain architectural boundaries.
        // Api only registers Application layer.
        services.AddCatalogApplication(configuration);
        services.AddHealthChecks().AddCheck<CatalogHealthCheck>("catalog");
        return services;
    }

    public static IEndpointRouteBuilder MapCatalogModule(this IEndpointRouteBuilder app)
    {
        // Catalog routes will be registered here in subsequent stories.
        return app;
    }
}
