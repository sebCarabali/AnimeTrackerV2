using AnimeTracker.Catalog.Application;
using AnimeTracker.Catalog.Contracts.Queries;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace AnimeTracker.Tracking.IntegrationTests;

/// <summary>
/// CA4: Tracking obtiene el resumen de un anime vía IMediator sin acceder a los internals de Catalog.
/// </summary>
public class MediatorCrossModuleTests
{
    private readonly IMediator _mediator;

    public MediatorCrossModuleTests()
    {
        var services = new ServiceCollection();
        services.AddCatalogApplication();
        // Tracking would register its own handlers here; omitted since it has none yet.
        var provider = services.BuildServiceProvider();
        _mediator = provider.GetRequiredService<IMediator>();
    }

    [Fact]
    public async Task Tracking_obtiene_resumen_de_anime_via_mediator()
    {
        var animeId = Guid.NewGuid();

        // Tracking sends the public contract query — it never calls Catalog's repositories directly.
        var result = await _mediator.Send(new GetAnimeSummaryQuery(animeId));

        result.ShouldNotBeNull();
        result.Id.ShouldBe(animeId);
        result.Title.ShouldNotBeNullOrEmpty();
    }

    [Fact]
    public async Task Tracking_no_necesita_importar_internals_de_Catalog_para_obtener_resumen()
    {
        // If this test compiles without referencing AnimeTracker.Catalog.Domain or
        // AnimeTracker.Catalog.Infrastructure, the module boundary is respected.
        var animeId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var query = new GetAnimeSummaryQuery(animeId);

        var result = await _mediator.Send(query);

        result.ShouldNotBeNull();
        result!.Id.ShouldBe(animeId);
    }
}
