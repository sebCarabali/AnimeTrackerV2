using AnimeTracker.Calendar.Application;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace AnimeTracker.Calendar.IntegrationTests;

public sealed class CalendarApplicationStartupTests
{
    [Fact]
    public void Application_services_register_without_error()
    {
        var provider = new ServiceCollection()
            .AddCalendarApplication()
            .BuildServiceProvider();

        provider.ShouldNotBeNull();
    }
}
