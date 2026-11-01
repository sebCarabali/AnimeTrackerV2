using AnimeTracker.Calendar.Domain;
using Shouldly;

namespace AnimeTracker.Calendar.UnitTests;

public sealed class CalendarDomainAssemblyTests
{
    [Fact]
    public void Domain_assembly_is_named_correctly()
    {
        typeof(Placeholder).Assembly.GetName().Name
            .ShouldBe("AnimeTracker.Calendar.Domain");
    }
}
