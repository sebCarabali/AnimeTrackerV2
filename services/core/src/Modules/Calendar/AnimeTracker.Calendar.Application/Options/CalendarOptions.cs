using System.ComponentModel.DataAnnotations;

namespace AnimeTracker.Calendar.Application.Options;

public class CalendarOptions
{
    public const string SectionName = "Calendar";

    [Required(ErrorMessage = "ConnectionString is required")]
    public string ConnectionString { get; set; } = string.Empty;

    [Range(1, 10, ErrorMessage = "MaxRetries must be 1-10")]
    public int MaxRetries { get; set; } = 3;
}
