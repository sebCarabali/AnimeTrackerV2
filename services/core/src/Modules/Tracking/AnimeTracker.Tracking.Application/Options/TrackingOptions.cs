using System.ComponentModel.DataAnnotations;

namespace AnimeTracker.Tracking.Application.Options;

public class TrackingOptions
{
    public const string SectionName = "Tracking";

    [Required(ErrorMessage = "ConnectionString is required")]
    public string ConnectionString { get; set; } = string.Empty;

    [Range(1, 10, ErrorMessage = "MaxRetries must be 1-10")]
    public int MaxRetries { get; set; } = 3;
}
