using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using AnimeTracker.Testing;
using Microsoft.AspNetCore.Mvc.Testing;
using Shouldly;

namespace AnimeTracker.Host.IntegrationTests;

public class ContractTests(WebApplicationFactory<Program> factory)
    : IClassFixture<WebApplicationFactory<Program>>
{
    private const string CoreSpec = "contracts/internal/core.v1.yaml";

    [Fact]
    public async Task Health_responde_segun_contrato()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("/health");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        await ContractAssert.SatisfiesAsync(CoreSpec, "GET", "/health", response);
    }

    [Fact]
    public async Task Health_respuesta_sin_campo_obligatorio_falla_el_contrato()
    {
        // Build a fake response that is missing the required "status" field
        var incompleteBody = JsonSerializer.Serialize(new { timestamp = "2026-10-31T20:00:00Z" });
        var fakeResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(incompleteBody, System.Text.Encoding.UTF8, "application/json"),
        };

        var ex = await Should.ThrowAsync<ContractViolationException>(
            () => ContractAssert.SatisfiesAsync(CoreSpec, "GET", "/health", fakeResponse));

        ex.Message.ShouldContain("status");
    }

    [Fact]
    public async Task Health_respuesta_con_tipo_incorrecto_falla_el_contrato()
    {
        // "status" must be a string; send an integer instead
        var wrongTypeBody = JsonSerializer.Serialize(new { status = 42, timestamp = "2026-10-31T20:00:00Z" });
        var fakeResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(wrongTypeBody, System.Text.Encoding.UTF8, "application/json"),
        };

        var ex = await Should.ThrowAsync<ContractViolationException>(
            () => ContractAssert.SatisfiesAsync(CoreSpec, "GET", "/health", fakeResponse));

        ex.Message.ShouldContain("status");
    }
}
