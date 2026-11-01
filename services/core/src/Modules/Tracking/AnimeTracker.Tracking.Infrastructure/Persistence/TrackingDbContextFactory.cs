using AnimeTracker.SharedKernel;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace AnimeTracker.Tracking.Infrastructure.Persistence;

/// <summary>
/// Design-time factory for creating TrackingDbContext instances.
/// Used by Entity Framework tools (migrations, etc) without needing appsettings.json.
/// </summary>
public class TrackingDbContextFactory : IDesignTimeDbContextFactory<TrackingDbContext>
{
    /// <summary>
    /// Creates a TrackingDbContext instance for design-time operations.
    /// </summary>
    public TrackingDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<TrackingDbContext>();

        // Use a local PostgreSQL database for design-time migrations
        var connectionString = "Host=localhost;Port=5432;Database=anime_tracker_tracking;Username=postgres;Password=postgres";

        optionsBuilder
            .ApplyCommonConventions()
            .UseNpgsql(
                connectionString,
                npgsqlOptions => npgsqlOptions
                    .MigrationsAssembly("AnimeTracker.Tracking.Infrastructure")
                    .MigrationsHistoryTable("__ef_migrations_history", "tracking")
            );

        return new TrackingDbContext(optionsBuilder.Options);
    }
}
