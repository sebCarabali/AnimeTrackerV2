using AnimeTracker.Catalog.Contracts.Queries;
using MediatR;

namespace AnimeTracker.Catalog.Application.Queries;

public sealed class GetAnimeSummaryQueryHandler : IRequestHandler<GetAnimeSummaryQuery, AnimeSummaryDto?>
{
    // Stub: returns a hard-coded summary. Real impl will query the Catalog DB (HU-00.08).
    public Task<AnimeSummaryDto?> Handle(GetAnimeSummaryQuery request, CancellationToken cancellationToken)
    {
        var summary = new AnimeSummaryDto(
            request.AnimeId,
            "Stub Anime Title",
            null);
        return Task.FromResult<AnimeSummaryDto?>(summary);
    }
}
