using AnimeTracker.SharedKernel;
using AnimeTracker.Tracking.Domain;
using Microsoft.EntityFrameworkCore;

namespace AnimeTracker.Tracking.Infrastructure.Persistence;

/// <summary>
/// DbContext para el módulo Tracking (WatchHistory).
/// Maneja la persistencia del historial de visualización de animes en el esquema "tracking".
/// </summary>
public class TrackingDbContext : DbContext
{
    /// <summary>
    /// Conjunto de registros de historial de visualización.
    /// </summary>
    public DbSet<WatchHistory> WatchHistories => Set<WatchHistory>();

    /// <summary>
    /// Constructor que recibe opciones de DbContext.
    /// </summary>
    public TrackingDbContext(DbContextOptions<TrackingDbContext> options)
        : base(options)
    {
    }

    /// <summary>
    /// Configura el modelo EF Core: esquema, mapeos de entidades, convenciones.
    /// </summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Esquema por defecto para este DbContext
        modelBuilder.HasDefaultSchema("tracking");

        // Aplicar convenciones comunes (timestamptz para DateTime)
        modelBuilder.ConfigureDateTimeProperties();

        // Mapear entidad WatchHistory
        ConfigureWatchHistory(modelBuilder);
    }

    /// <summary>
    /// Configura el mapeo de la entidad WatchHistory.
    /// </summary>
    private static void ConfigureWatchHistory(ModelBuilder modelBuilder)
    {
        var watchHistoryBuilder = modelBuilder.Entity<WatchHistory>();

        watchHistoryBuilder.HasKey(e => e.Id);

        watchHistoryBuilder.Property(e => e.UserId)
            .IsRequired()
            .HasMaxLength(255);

        watchHistoryBuilder.Property(e => e.AnimeId)
            .IsRequired();

        watchHistoryBuilder.Property(e => e.Status)
            .IsRequired()
            .HasMaxLength(50);

        watchHistoryBuilder.Property(e => e.EpisodesWatched)
            .IsRequired();

        watchHistoryBuilder.Property(e => e.TotalEpisodes)
            .IsRequired(false);

        watchHistoryBuilder.Property(e => e.CreatedAt)
            .IsRequired();

        watchHistoryBuilder.Property(e => e.UpdatedAt)
            .IsRequired(false);

        // Índices para búsquedas comunes
        watchHistoryBuilder.HasIndex(e => new { e.UserId, e.AnimeId })
            .IsUnique();

        watchHistoryBuilder.HasIndex(e => e.UserId);

        watchHistoryBuilder.ToTable("watch_histories");
    }
}
