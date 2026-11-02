using AnimeTracker.Calendar.Api;
using AnimeTracker.Calendar.Infrastructure;
using AnimeTracker.Catalog.Api;
using AnimeTracker.Catalog.Infrastructure;
using AnimeTracker.Tracking.Api;
using AnimeTracker.Tracking.Infrastructure;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Net;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Register module APIs (Application layer)
builder.Services
    .AddCatalogModule(builder.Configuration)
    .AddTrackingModule(builder.Configuration)
    .AddCalendarModule(builder.Configuration);

// Register Infrastructure layer (Host responsibility, not Api)
builder.Services
    .AddCatalogInfrastructure(builder.Configuration)
    .AddTrackingInfrastructure(builder.Configuration)
    .AddCalendarInfrastructure(builder.Configuration);

builder.Services.AddOpenApi();

// Proxy IPs come from "ForwardedHeadersOptions:KnownProxies" in appsettings.{Env}.json.
// Replace the loopback guard below with RequireAuthorization() once EP-01 lands.
builder.Services.AddOptions<ForwardedHeadersOptions>()
    .Configure<IConfiguration>((options, config) =>
    {
        options.ForwardedHeaders = ForwardedHeaders.XForwardedFor;
        foreach (var ip in config
            .GetSection("ForwardedHeadersOptions:KnownProxies")
            .GetChildren()
            .Select(x => IPAddress.TryParse(x.Value, out var addr) ? addr : null)
            .OfType<IPAddress>())
            options.KnownProxies.Add(ip);
    });

var app = builder.Build();

// Auto-migrate databases on startup
using (var scope = app.Services.CreateScope())
{
    var serviceProvider = scope.ServiceProvider;
    try
    {
        var catalogDb = serviceProvider.GetRequiredService<AnimeTracker.Catalog.Infrastructure.Persistence.CatalogDbContext>();
        await catalogDb.Database.MigrateAsync();

        var trackingDb = serviceProvider.GetRequiredService<AnimeTracker.Tracking.Infrastructure.Persistence.TrackingDbContext>();
        await trackingDb.Database.MigrateAsync();

        var calendarDb = serviceProvider.GetRequiredService<AnimeTracker.Calendar.Infrastructure.Persistence.CalendarDbContext>();
        await calendarDb.Database.MigrateAsync();
    }
    catch (Exception ex)
    {
        var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while migrating databases during startup");
        throw;
    }
}

if (app.Environment.IsDevelopment())
    app.MapOpenApi();

// Populate RemoteIpAddress from X-Forwarded-For when behind a trusted proxy.
// Trusted proxies: loopback by default + those in "ForwardedHeadersOptions:KnownProxies".
app.UseForwardedHeaders();

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
            status    = report.Status.ToString(),
            timestamp = DateTime.UtcNow.ToString("o"),
            entries   = report.Entries.ToDictionary(
                e => e.Key,
                e => new { status = e.Value.Status.ToString(), description = e.Value.Description })
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
