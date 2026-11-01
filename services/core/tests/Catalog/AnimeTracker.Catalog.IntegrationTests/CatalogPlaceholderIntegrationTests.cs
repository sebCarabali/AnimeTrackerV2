using AnimeTracker.Catalog.Application;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace AnimeTracker.Catalog.IntegrationTests;

public sealed class CatalogApplicationStartupTests
{
    [Fact]
    public void Application_services_register_without_error()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Catalog:ConnectionString"] = "test"
            })
            .Build();

        var provider = new ServiceCollection()
            .AddCatalogApplication(configuration)
            .BuildServiceProvider();

        provider.ShouldNotBeNull();
    }
}
