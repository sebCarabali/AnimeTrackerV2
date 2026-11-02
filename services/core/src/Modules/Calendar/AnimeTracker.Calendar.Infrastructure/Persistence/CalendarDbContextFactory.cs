using AnimeTracker.SharedKernel;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace AnimeTracker.Calendar.Infrastructure.Persistence;

/// <summary>
/// Design-time factory for creating CalendarDbContext instances.
/// Used by Entity Framework tools (migrations, etc) without needing appsettings.json.
/// </summary>
public class CalendarDbContextFactory : IDesignTimeDbContextFactory<CalendarDbContext>
{
    /// <summary>
    /// Creates a CalendarDbContext instance for design-time operations.
    /// </summary>
    public CalendarDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<CalendarDbContext>();

        // Use a local PostgreSQL database for design-time migrations
        var connectionString = "Host=localhost;Port=5432;Database=anime_tracker_calendar;Username=postgres;Password=postgres";

        optionsBuilder
            .ApplyCommonConventions()
            .UseNpgsql(
                connectionString,
                npgsqlOptions => npgsqlOptions
                    .MigrationsAssembly("AnimeTracker.Calendar.Infrastructure")
                    .MigrationsHistoryTable("__ef_migrations_history", "calendar")
            );

        return new CalendarDbContext(optionsBuilder.Options);
    }
}
