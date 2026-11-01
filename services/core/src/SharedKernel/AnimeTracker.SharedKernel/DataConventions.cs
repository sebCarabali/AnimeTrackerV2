using Microsoft.EntityFrameworkCore;

namespace AnimeTracker.SharedKernel;

/// <summary>
/// Extensiones y convenciones comunes para EF Core en todos los módulos.
/// </summary>
public static class DataConventions
{
    /// <summary>
    /// Aplica convenciones comunes en DbContextOptionsBuilder:
    /// - Nombres de tablas/columnas en snake_case.
    /// </summary>
    public static DbContextOptionsBuilder ApplyCommonConventions(this DbContextOptionsBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        // snake_case para tablas y columnas
        builder.UseSnakeCaseNamingConvention();

        return builder;
    }

    /// <summary>
    /// Configura el mapeo de propiedades DateTime a timestamptz en ModelBuilder.
    /// </summary>
    public static void ConfigureDateTimeProperties(this ModelBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        // Mapeo de DateTime a timestamptz (para PostgreSQL)
        var dateTimeProperties = builder.Model
            .GetEntityTypes()
            .SelectMany(t => t.GetProperties())
            .Where(p => p.ClrType == typeof(DateTime) || p.ClrType == typeof(DateTime?));

        foreach (var property in dateTimeProperties)
        {
            property.SetColumnType("timestamptz");
        }
    }
}
