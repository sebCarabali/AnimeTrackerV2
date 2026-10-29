namespace AnimeTracker.Catalog.Contracts.Queries;

public sealed record AnimeSummaryDto(Guid Id, string Title, string? CoverImageUrl);
