using System.Reflection;

namespace AnimeTracker.ArchitectureTests;

internal static class AssemblyHelper
{
    private static readonly Dictionary<string, Assembly> _assemblies = new()
    {
        [$"Catalog.Domain"]         = typeof(AnimeTracker.Catalog.Domain.Placeholder).Assembly,
        [$"Catalog.Application"]    = typeof(AnimeTracker.Catalog.Application.Placeholder).Assembly,
        [$"Catalog.Infrastructure"] = typeof(AnimeTracker.Catalog.Infrastructure.Placeholder).Assembly,
        [$"Catalog.Api"]            = typeof(AnimeTracker.Catalog.Api.Placeholder).Assembly,
        [$"Catalog.Contracts"]      = typeof(AnimeTracker.Catalog.Contracts.Placeholder).Assembly,
        [$"Tracking.Domain"]        = typeof(AnimeTracker.Tracking.Domain.Placeholder).Assembly,
        [$"Tracking.Application"]   = typeof(AnimeTracker.Tracking.Application.Placeholder).Assembly,
        [$"Tracking.Infrastructure"]= typeof(AnimeTracker.Tracking.Infrastructure.Placeholder).Assembly,
        [$"Tracking.Api"]           = typeof(AnimeTracker.Tracking.Api.Placeholder).Assembly,
        [$"Tracking.Contracts"]     = typeof(AnimeTracker.Tracking.Contracts.Placeholder).Assembly,
        [$"Calendar.Domain"]        = typeof(AnimeTracker.Calendar.Domain.Placeholder).Assembly,
        [$"Calendar.Application"]   = typeof(AnimeTracker.Calendar.Application.Placeholder).Assembly,
        [$"Calendar.Infrastructure"]= typeof(AnimeTracker.Calendar.Infrastructure.Placeholder).Assembly,
        [$"Calendar.Api"]           = typeof(AnimeTracker.Calendar.Api.Placeholder).Assembly,
        [$"Calendar.Contracts"]     = typeof(AnimeTracker.Calendar.Contracts.Placeholder).Assembly,
    };

    internal static Assembly Of(string module, string layer) =>
        _assemblies[$"{module}.{layer}"];
}
