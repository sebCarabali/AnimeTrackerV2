using AnimeTracker.Calendar.Infrastructure.Persistence;
using AnimeTracker.SharedKernel;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AnimeTracker.Calendar.Infrastructure;

/// <summary>
/// Extensiones para registrar servicios de infraestructura del módulo Calendar.
/// </summary>
public static class CalendarInfrastructureExtensions
{
    /// <summary>
    /// Registra los servicios de infraestructura del módulo Calendar.
    /// Incluye el DbContext configurado para PostgreSQL.
    /// </summary>
    /// <param name="services">Colección de servicios.</param>
    /// <param name="configuration">Configuración de la aplicación.</param>
    /// <returns>La misma colección de servicios para encadenamiento.</returns>
    public static IServiceCollection AddCalendarInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Postgres")
            ?? throw new InvalidOperationException("Connection string 'Postgres' not found in configuration.");

        services.AddDbContext<CalendarDbContext>(options =>
            options
                .ApplyCommonConventions()
                .UseNpgsql(
                    connectionString,
                    npgsqlOptions => npgsqlOptions
                        .MigrationsAssembly("AnimeTracker.Calendar.Infrastructure")
                        .MigrationsHistoryTable("__ef_migrations_history", "calendar")
                )
        );

        return services;
    }
}
