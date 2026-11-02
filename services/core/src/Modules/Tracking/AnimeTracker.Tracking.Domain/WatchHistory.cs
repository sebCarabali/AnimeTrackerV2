using AnimeTracker.SharedKernel;

namespace AnimeTracker.Tracking.Domain;

/// <summary>
/// Representa el historial de visualización de un anime por un usuario.
/// </summary>
public class WatchHistory : AggregateRoot<int>
{
    /// <summary>
    /// ID del usuario que ve el anime.
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// ID del anime siendo visto.
    /// </summary>
    public int AnimeId { get; set; }

    /// <summary>
    /// Estado del seguimiento: "Viendo", "Completado", "En Espera", "Abandonado", "Plan para ver".
    /// </summary>
    public string Status { get; set; } = "Plan para ver";

    /// <summary>
    /// Número de episodios vistos.
    /// </summary>
    public int EpisodesWatched { get; set; }

    /// <summary>
    /// Total de episodios del anime (puede ser nulo si está en emisión).
    /// </summary>
    public int? TotalEpisodes { get; set; }

    /// <summary>
    /// Fecha de creación del registro.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Última actualización del seguimiento.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Constructor privado para EF Core.
    /// </summary>
#pragma warning disable CS8618
    private WatchHistory() { }
#pragma warning restore CS8618

    /// <summary>
    /// Factory method para crear un nuevo registro de seguimiento.
    /// </summary>
    public static WatchHistory Create(string userId, int animeId, string status = "Plan para ver")
    {
        return new WatchHistory
        {
            UserId = userId,
            AnimeId = animeId,
            Status = status,
            EpisodesWatched = 0,
            CreatedAt = DateTime.UtcNow
        };
    }
}
