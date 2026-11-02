using System.ComponentModel.DataAnnotations;

namespace AnimeTracker.Catalog.Application.Options;

public class CatalogOptions
{
    public const string SectionName = "Catalog";

    [Required(ErrorMessage = "ConnectionString is required")]
    public string ConnectionString { get; set; } = string.Empty;

    [Range(1, 10, ErrorMessage = "MaxRetries must be 1-10")]
    public int MaxRetries { get; set; } = 3;
}
