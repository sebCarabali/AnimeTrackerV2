using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using System.Net;
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

        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        root.GetProperty("status").GetString().ShouldBe("Healthy");
        root.GetProperty("timestamp").GetString().ShouldNotBeNullOrEmpty();

        var entries = root.GetProperty("entries");
        entries.TryGetProperty("catalog", out _).ShouldBeTrue();
        entries.TryGetProperty("tracking", out _).ShouldBeTrue();
        entries.TryGetProperty("calendar", out _).ShouldBeTrue();
    }

    [Fact]
    public async Task Health_rechaza_peticion_desde_ip_no_loopback()
    {
        await using var testFactory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
                builder.ConfigureServices(services =>
                    services.AddSingleton<IStartupFilter>(
                        new FakeRemoteIpStartupFilter(IPAddress.Parse("203.0.113.1")))));

        using var client = testFactory.CreateClient();
        var response = await client.GetAsync("/health");

        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Health_permite_acceso_via_proxy_conocido_con_X_Forwarded_For_loopback()
    {
        const string proxyIp = "10.0.0.1";
        await using var testFactory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureAppConfiguration((_, cfg) =>
                    cfg.AddInMemoryCollection(new Dictionary<string, string?>
                    {
                        ["ForwardedHeadersOptions:KnownProxies:0"] = proxyIp
                    }));
                builder.ConfigureServices(services =>
                    services.AddSingleton<IStartupFilter>(
                        new FakeRemoteIpStartupFilter(IPAddress.Parse(proxyIp))));
            });

        using var client = testFactory.CreateClient();
        var request = new HttpRequestMessage(HttpMethod.Get, "/health");
        request.Headers.Add("X-Forwarded-For", "127.0.0.1");
        var response = await client.SendAsync(request);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }
}

file sealed class FakeRemoteIpStartupFilter(IPAddress ip) : IStartupFilter
{
    public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next) =>
        app =>
        {
            app.Use(async (ctx, next2) =>
            {
                ctx.Connection.RemoteIpAddress = ip;
                await next2(ctx);
            });
            next(app);
        };
}
