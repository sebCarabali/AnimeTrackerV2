using AnimeTracker.SharedKernel;

namespace AnimeTracker.Catalog.Domain;

/// <summary>
/// Representa un anime en el catálogo.
/// </summary>
public class Anime : AggregateRoot<int>
{
    /// <summary>
    /// Título del anime.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// ID externo de la API de metadatos (Jikan, AniList, etc).
    /// </summary>
    public string ExternalId { get; set; } = string.Empty;

    /// <summary>
    /// Descripción/sinopsis del anime.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Fecha de creación de esta entidad en el sistema.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Última actualización de esta entidad.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Constructor privado para EF Core.
    /// </summary>
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    private Anime() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

    /// <summary>
    /// Factory method para crear un nuevo Anime.
    /// </summary>
    public static Anime Create(string title, string externalId, string? description = null)
    {
        return new Anime
        {
            Title = title,
            ExternalId = externalId,
            Description = description,
            CreatedAt = DateTime.UtcNow
        };
    }
}
