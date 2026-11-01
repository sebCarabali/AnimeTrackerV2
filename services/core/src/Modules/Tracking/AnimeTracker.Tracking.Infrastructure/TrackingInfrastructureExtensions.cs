using AnimeTracker.SharedKernel;
using AnimeTracker.Tracking.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AnimeTracker.Tracking.Infrastructure;

/// <summary>
/// Extensiones para registrar servicios de infraestructura del módulo Tracking.
/// </summary>
public static class TrackingInfrastructureExtensions
{
    /// <summary>
    /// Registra los servicios de infraestructura del módulo Tracking.
    /// Incluye el DbContext configurado para PostgreSQL.
    /// </summary>
    /// <param name="services">Colección de servicios.</param>
    /// <param name="configuration">Configuración de la aplicación.</param>
    /// <returns>La misma colección de servicios para encadenamiento.</returns>
    public static IServiceCollection AddTrackingInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Postgres")
            ?? throw new InvalidOperationException("Connection string 'Postgres' not found in configuration.");

        services.AddDbContext<TrackingDbContext>(options =>
            options
                .ApplyCommonConventions()
                .UseNpgsql(
                    connectionString,
                    npgsqlOptions => npgsqlOptions
                        .MigrationsAssembly("AnimeTracker.Tracking.Infrastructure")
                        .MigrationsHistoryTable("__ef_migrations_history", "tracking")
                )
        );

        return services;
    }
}
