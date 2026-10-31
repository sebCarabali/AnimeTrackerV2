using AnimeTracker.Tracking.Domain;
using Shouldly;

namespace AnimeTracker.Tracking.UnitTests;

public sealed class TrackingDomainAssemblyTests
{
    [Fact]
    public void Domain_assembly_is_named_correctly()
    {
        typeof(Placeholder).Assembly.GetName().Name
            .ShouldBe("AnimeTracker.Tracking.Domain");
    }
}
