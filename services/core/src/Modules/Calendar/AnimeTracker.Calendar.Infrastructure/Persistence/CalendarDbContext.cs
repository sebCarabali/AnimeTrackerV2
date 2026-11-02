using AnimeTracker.Calendar.Domain;
using AnimeTracker.SharedKernel;
using Microsoft.EntityFrameworkCore;

namespace AnimeTracker.Calendar.Infrastructure.Persistence;

/// <summary>
/// DbContext para el módulo Calendar.
/// Maneja la persistencia de lanzamientos de episodios en el esquema "calendar".
/// </summary>
public class CalendarDbContext : DbContext
{
    /// <summary>
    /// Conjunto de lanzamientos de episodios.
    /// </summary>
    public DbSet<AnimeRelease> AnimeReleases => Set<AnimeRelease>();

    /// <summary>
    /// Constructor que recibe opciones de DbContext.
    /// </summary>
    public CalendarDbContext(DbContextOptions<CalendarDbContext> options)
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
        modelBuilder.HasDefaultSchema("calendar");

        // Aplicar convenciones comunes (timestamptz para DateTime)
        modelBuilder.ConfigureDateTimeProperties();

        // Mapear entidad AnimeRelease
        ConfigureAnimeRelease(modelBuilder);
    }

    /// <summary>
    /// Configura el mapeo de la entidad AnimeRelease.
    /// </summary>
    private static void ConfigureAnimeRelease(ModelBuilder modelBuilder)
    {
        var animeReleaseBuilder = modelBuilder.Entity<AnimeRelease>();

        animeReleaseBuilder.HasKey(e => e.Id);

        animeReleaseBuilder.Property(e => e.AnimeId)
            .IsRequired();

        animeReleaseBuilder.Property(e => e.EpisodeNumber)
            .IsRequired();

        animeReleaseBuilder.Property(e => e.EpisodeTitle)
            .HasMaxLength(255);

        animeReleaseBuilder.Property(e => e.AirDateTime)
            .IsRequired();

        animeReleaseBuilder.Property(e => e.IsAired)
            .IsRequired();

        animeReleaseBuilder.Property(e => e.CreatedAt)
            .IsRequired();

        animeReleaseBuilder.Property(e => e.UpdatedAt)
            .IsRequired(false);

        // Índices para búsquedas comunes
        animeReleaseBuilder.HasIndex(e => new { e.AnimeId, e.EpisodeNumber })
            .IsUnique();

        animeReleaseBuilder.HasIndex(e => e.AirDateTime);

        animeReleaseBuilder.ToTable("anime_releases");
    }
}
