using AnimeTracker.Catalog.Application;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace AnimeTracker.Catalog.IntegrationTests;

public sealed class CatalogApplicationStartupTests
{
    [Fact]
    public void Application_services_register_without_error()
    {
        var provider = new ServiceCollection()
            .AddCatalogApplication()
            .BuildServiceProvider();

        provider.ShouldNotBeNull();
    }
}
