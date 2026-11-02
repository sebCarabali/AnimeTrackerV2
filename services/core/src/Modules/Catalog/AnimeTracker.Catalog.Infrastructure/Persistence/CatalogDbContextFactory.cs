using AnimeTracker.SharedKernel;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace AnimeTracker.Catalog.Infrastructure.Persistence;

/// <summary>
/// Design-time factory for creating CatalogDbContext instances.
/// Used by Entity Framework tools (migrations, etc) without needing appsettings.json.
/// </summary>
public class CatalogDbContextFactory : IDesignTimeDbContextFactory<CatalogDbContext>
{
    /// <summary>
    /// Creates a CatalogDbContext instance for design-time operations.
    /// </summary>
    public CatalogDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<CatalogDbContext>();

        // Use a local PostgreSQL database for design-time migrations
        var connectionString = "Host=localhost;Port=5432;Database=anime_tracker_catalog;Username=postgres;Password=postgres";

        optionsBuilder
            .ApplyCommonConventions()
            .UseNpgsql(
                connectionString,
                npgsqlOptions => npgsqlOptions
                    .MigrationsAssembly("AnimeTracker.Catalog.Infrastructure")
                    .MigrationsHistoryTable("__ef_migrations_history", "catalog")
            );

        return new CatalogDbContext(optionsBuilder.Options);
    }
}
