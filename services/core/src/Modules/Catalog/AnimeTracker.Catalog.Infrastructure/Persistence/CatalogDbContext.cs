using AnimeTracker.Catalog.Domain;
using AnimeTracker.SharedKernel;
using Microsoft.EntityFrameworkCore;

namespace AnimeTracker.Catalog.Infrastructure.Persistence;

/// <summary>
/// DbContext para el módulo Catalog.
/// Maneja la persistencia de animes en el esquema "catalog".
/// </summary>
public class CatalogDbContext : DbContext
{
    /// <summary>
    /// Conjunto de animes en el catálogo.
    /// </summary>
    public DbSet<Anime> Animes => Set<Anime>();

    /// <summary>
    /// Constructor que recibe opciones de DbContext.
    /// </summary>
    public CatalogDbContext(DbContextOptions<CatalogDbContext> options)
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
        modelBuilder.HasDefaultSchema("catalog");

        // Aplicar convenciones comunes (timestamptz para DateTime)
        modelBuilder.ConfigureDateTimeProperties();

        // Mapear entidad Anime
        ConfigureAnime(modelBuilder);
    }

    /// <summary>
    /// Configura el mapeo de la entidad Anime.
    /// </summary>
    private static void ConfigureAnime(ModelBuilder modelBuilder)
    {
        var animeBuilder = modelBuilder.Entity<Anime>();

        animeBuilder.HasKey(e => e.Id);

        animeBuilder.Property(e => e.Title)
            .IsRequired()
            .HasMaxLength(255);

        animeBuilder.Property(e => e.ExternalId)
            .IsRequired()
            .HasMaxLength(50);

        animeBuilder.Property(e => e.Description)
            .HasMaxLength(2000);

        animeBuilder.Property(e => e.CreatedAt)
            .IsRequired();

        animeBuilder.Property(e => e.UpdatedAt)
            .IsRequired(false);

        // Índices para búsquedas comunes
        animeBuilder.HasIndex(e => e.ExternalId)
            .IsUnique();

        animeBuilder.ToTable("animes");
    }
}
