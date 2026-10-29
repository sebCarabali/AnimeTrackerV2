using AnimeTracker.Catalog.Domain;
using Shouldly;

namespace AnimeTracker.Catalog.UnitTests;

public sealed class CatalogDomainAssemblyTests
{
    [Fact]
    public void Domain_assembly_is_named_correctly()
    {
        typeof(Placeholder).Assembly.GetName().Name
            .ShouldBe("AnimeTracker.Catalog.Domain");
    }
}
