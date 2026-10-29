using AnimeTracker.Calendar.Api;
using AnimeTracker.Catalog.Api;
using AnimeTracker.Tracking.Api;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddCatalogModule()
    .AddTrackingModule()
    .AddCalendarModule();

builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
    app.MapOpenApi();

app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = async (ctx, report) =>
    {
        ctx.Response.ContentType = "application/json";
        var result = JsonSerializer.Serialize(new
        {
            status = report.Status.ToString(),
            modules = report.Entries.Select(e => new
            {
                name   = e.Key,
                status = e.Value.Status.ToString(),
                description = e.Value.Description
            })
        });
        await ctx.Response.WriteAsync(result);
    }
});

app.MapCatalogModule();
app.MapTrackingModule();
app.MapCalendarModule();

app.Run();

// Needed by WebApplicationFactory in integration tests
public partial class Program;
