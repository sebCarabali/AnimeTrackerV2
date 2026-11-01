using AnimeTracker.SharedKernel;

namespace AnimeTracker.Calendar.Domain;

/// <summary>
/// Representa el lanzamiento de un episodio de un anime.
/// </summary>
public class AnimeRelease : AggregateRoot<int>
{
    /// <summary>
    /// ID del anime.
    /// </summary>
    public int AnimeId { get; set; }

    /// <summary>
    /// Número del episodio.
    /// </summary>
    public int EpisodeNumber { get; set; }

    /// <summary>
    /// Título del episodio.
    /// </summary>
    public string? EpisodeTitle { get; set; }

    /// <summary>
    /// Fecha y hora de lanzamiento en UTC.
    /// </summary>
    public DateTime AirDateTime { get; set; }

    /// <summary>
    /// Indica si el episodio ya ha sido lanzado.
    /// </summary>
    public bool IsAired { get; set; }

    /// <summary>
    /// Fecha de creación del registro.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Última actualización del registro.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Constructor privado para EF Core.
    /// </summary>
#pragma warning disable CS8618
    private AnimeRelease() { }
#pragma warning restore CS8618

    /// <summary>
    /// Factory method para crear un nuevo lanzamiento de episodio.
    /// </summary>
    public static AnimeRelease Create(int animeId, int episodeNumber, DateTime airDateTime, string? episodeTitle = null)
    {
        return new AnimeRelease
        {
            AnimeId = animeId,
            EpisodeNumber = episodeNumber,
            EpisodeTitle = episodeTitle,
            AirDateTime = airDateTime,
            IsAired = DateTime.UtcNow >= airDateTime,
            CreatedAt = DateTime.UtcNow
        };
    }
}
