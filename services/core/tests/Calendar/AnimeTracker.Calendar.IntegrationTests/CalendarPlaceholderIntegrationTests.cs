using AnimeTracker.Calendar.Application;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace AnimeTracker.Calendar.IntegrationTests;

public sealed class CalendarApplicationStartupTests
{
    [Fact]
    public void Application_services_register_without_error()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Calendar:ConnectionString"] = "test"
            })
            .Build();

        var provider = new ServiceCollection()
            .AddCalendarApplication(configuration)
            .BuildServiceProvider();

        provider.ShouldNotBeNull();
    }
}
