using Microsoft.AspNetCore.Mvc.Testing;
using Shouldly;
using System.Text.Json;

namespace AnimeTracker.Host.IntegrationTests;

public class HealthCheckTests(WebApplicationFactory<Program> factory)
    : IClassFixture<WebApplicationFactory<Program>>
{
    [Fact]
    public async Task Host_health_responde_200_con_modulos_registrados()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("/health");

        response.StatusCode.ShouldBe(System.Net.HttpStatusCode.OK);

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        root.GetProperty("status").GetString().ShouldBe("Healthy");

        var modules = root.GetProperty("modules").EnumerateArray().Select(m => m.GetProperty("name").GetString()).ToList();
        modules.ShouldContain("catalog");
        modules.ShouldContain("tracking");
        modules.ShouldContain("calendar");
    }
}
