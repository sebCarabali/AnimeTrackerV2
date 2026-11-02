using AnimeTracker.Catalog.Infrastructure.Persistence;
using AnimeTracker.SharedKernel;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AnimeTracker.Catalog.Infrastructure;

/// <summary>
/// Extensiones para registrar servicios de infraestructura del módulo Catalog.
/// </summary>
public static class CatalogInfrastructureExtensions
{
    /// <summary>
    /// Registra los servicios de infraestructura del módulo Catalog.
    /// Incluye el DbContext configurado para PostgreSQL.
    /// </summary>
    /// <param name="services">Colección de servicios.</param>
    /// <param name="configuration">Configuración de la aplicación.</param>
    /// <returns>La misma colección de servicios para encadenamiento.</returns>
    public static IServiceCollection AddCatalogInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Postgres")
            ?? throw new InvalidOperationException("Connection string 'Postgres' not found in configuration.");

        services.AddDbContext<CatalogDbContext>(options =>
            options
                .ApplyCommonConventions()
                .UseNpgsql(
                    connectionString,
                    npgsqlOptions => npgsqlOptions
                        .MigrationsAssembly("AnimeTracker.Catalog.Infrastructure")
                        .MigrationsHistoryTable("__ef_migrations_history", "catalog")
                )
        );

        return services;
    }
}
