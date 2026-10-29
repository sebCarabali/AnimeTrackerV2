using AnimeTracker.Calendar.Api;
using AnimeTracker.Catalog.Api;
using AnimeTracker.Tracking.Api;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Net;
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

// Restrict /health to loopback until auth is in place (OWASP A05/A01).
// null RemoteIpAddress = in-process transport (tests, local proxy); allow through.
app.UseWhen(
    ctx => ctx.Request.Path.StartsWithSegments("/health"),
    branch => branch.Use(async (ctx, next) =>
    {
        var ip = ctx.Connection.RemoteIpAddress;
        if (ip is not null && !IPAddress.IsLoopback(ip))
        {
            ctx.Response.StatusCode = StatusCodes.Status403Forbidden;
            return;
        }
        await next(ctx);
    }));

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
