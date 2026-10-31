using MediatR;

namespace AnimeTracker.Catalog.Contracts.Queries;

public sealed record GetAnimeSummaryQuery(Guid AnimeId) : IRequest<AnimeSummaryDto?>;
