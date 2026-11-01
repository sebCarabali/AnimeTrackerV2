using System.Runtime.CompilerServices;
using System.Text.Json;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

[assembly: InternalsVisibleTo("AnimeTracker.Host.IntegrationTests")]

namespace AnimeTracker.Testing;

/// <summary>
/// Validates that an HTTP response satisfies the schema defined in an OpenAPI contract file.
/// </summary>
public static class ContractAssert
{
    private static readonly IDeserializer YamlDeserializer = new DeserializerBuilder()
        .WithNamingConvention(NullNamingConvention.Instance)
        .Build();

    /// <summary>
    /// Asserts that <paramref name="response"/> matches the response schema declared in
    /// <paramref name="spec"/> for the given <paramref name="method"/> and <paramref name="path"/>.
    /// </summary>
    /// <param name="spec">Relative path from the repository root to the OpenAPI YAML file.</param>
    /// <param name="method">HTTP method (e.g. "GET").</param>
    /// <param name="path">API path (e.g. "/health").</param>
    /// <param name="response">The actual HTTP response to validate.</param>
    public static async Task SatisfiesAsync(
        string spec,
        string method,
        string path,
        HttpResponseMessage response)
    {
        var specPath = ResolveSpecPath(spec);
        var yaml = await File.ReadAllTextAsync(specPath);
        var specDoc = (IDictionary<object, object>)YamlDeserializer.Deserialize(new StringReader(yaml))!;

        var paths = GetDict(specDoc, "paths")
            ?? throw new ContractViolationException($"No 'paths' found in contract '{spec}'.");

        if (!paths.TryGetValue(path, out var pathObj))
            throw new ContractViolationException($"Path '{path}' not found in contract '{spec}'.");

        var pathItem = CastDict(pathObj, $"path '{path}'");
        var methodKey = method.ToLowerInvariant();

        if (!pathItem.TryGetValue(methodKey, out var operationObj))
            throw new ContractViolationException(
                $"Method '{method}' not found for path '{path}' in contract '{spec}'.");

        var operation = CastDict(operationObj, $"{method} {path}");
        var responses = GetDict(operation, "responses")
            ?? throw new ContractViolationException($"No 'responses' for {method} {path} in contract '{spec}'.");

        var statusCode = ((int)response.StatusCode).ToString();
        if (!responses.TryGetValue(statusCode, out var responseObj))
            throw new ContractViolationException(
                $"Status {statusCode} not declared for {method} {path} in contract '{spec}'.");

        var responseSpec = CastDict(responseObj, $"response {statusCode}");
        var content = GetDict(responseSpec, "content");
        // H3: read body first so we can detect when body exists but contract declares no content
        var body = await response.Content.ReadAsStringAsync();

        if (content is null)
        {
            if (!string.IsNullOrWhiteSpace(body))
                throw new ContractViolationException(
                    $"Response has a body but contract declares no 'content' for {statusCode} {method} {path} in '{spec}'.");
            return;
        }

        if (string.IsNullOrWhiteSpace(body))
            return;

        var contentType = response.Content.Headers.ContentType?.MediaType ?? "application/json";
        IDictionary<object, object>? mediaTypeSpec = null;
        foreach (var kv in content)
        {
            if (kv.Key.ToString()!.StartsWith(contentType, StringComparison.OrdinalIgnoreCase))
            {
                mediaTypeSpec = CastDict(kv.Value, $"content '{kv.Key}'");
                break;
            }
        }

        if (mediaTypeSpec is null)
            throw new ContractViolationException(
                $"Content-Type '{contentType}' not declared for {statusCode} {method} {path} in contract '{spec}'.");

        var schemaObj = mediaTypeSpec.TryGetValue("schema", out var s) ? s : null;
        if (schemaObj is null)
            return;

        var schema = CastDict(schemaObj, "schema");
        var components = GetDict(specDoc, "components");
        using var jsonDoc = JsonDocument.Parse(body);
        ValidateSchema(schema, specDoc, components, jsonDoc.RootElement, "$");
    }

    private static void ValidateSchema(
        IDictionary<object, object> schema,
        IDictionary<object, object> specDoc,
        IDictionary<object, object>? components,
        JsonElement element,
        string path)
    {
        schema = ResolveRef(schema, specDoc, components);

        // Type check
        if (schema.TryGetValue("type", out var typeObj) && typeObj is string expectedType)
        {
            var ok = expectedType switch
            {
                "object"  => element.ValueKind == JsonValueKind.Object,
                "array"   => element.ValueKind == JsonValueKind.Array,
                "string"  => element.ValueKind == JsonValueKind.String,
                // H2: integer must be a whole number, not any JSON number (e.g. 1.5 must fail)
                "integer" => element.ValueKind == JsonValueKind.Number && element.TryGetInt64(out _),
                "number"  => element.ValueKind == JsonValueKind.Number,
                "boolean" => element.ValueKind is JsonValueKind.True or JsonValueKind.False,
                _         => true
            };

            if (!ok)
                throw new ContractViolationException(
                    $"Type mismatch at '{path}': expected '{expectedType}' but got '{element.ValueKind}'.");
        }

        if (element.ValueKind != JsonValueKind.Object)
            return;

        // Required properties
        if (schema.TryGetValue("required", out var req) && req is IList<object> required)
        {
            foreach (var field in required)
            {
                if (!element.TryGetProperty(field.ToString()!, out _))
                    throw new ContractViolationException(
                        $"Missing required property '{field}' at '{path}'.");
            }
        }

        // Recurse into declared properties
        if (schema.TryGetValue("properties", out var propsObj) && propsObj is IDictionary<object, object> props)
        {
            foreach (var kv in props)
            {
                var propName = kv.Key.ToString()!;
                if (element.TryGetProperty(propName, out var propElement))
                {
                    var propSchema = CastDict(kv.Value, $"property '{propName}' schema");
                    ValidateSchema(propSchema, specDoc, components, propElement, $"{path}.{propName}");
                }
            }
        }
    }

    private static IDictionary<object, object> ResolveRef(
        IDictionary<object, object> schema,
        IDictionary<object, object> specDoc,
        IDictionary<object, object>? components)
    {
        if (!schema.TryGetValue("$ref", out var refObj) || refObj is not string refStr)
            return schema;

        // e.g. '#/components/schemas/CoreHealthStatus' → split on '/'
        var parts = refStr.TrimStart('#').Split('/', StringSplitOptions.RemoveEmptyEntries);
        IDictionary<object, object> current = specDoc;
        foreach (var part in parts)
        {
            if (!current.TryGetValue(part, out var next))
                throw new ContractViolationException($"Cannot resolve $ref '{refStr}': '{part}' not found.");
            current = CastDict(next, $"$ref segment '{part}'");
        }
        return current;
    }

    private static IDictionary<object, object>? GetDict(IDictionary<object, object> parent, string key)
        => parent.TryGetValue(key, out var v) && v is IDictionary<object, object> d ? d : null;

    private static IDictionary<object, object> CastDict(object? obj, string context)
        => obj as IDictionary<object, object>
           ?? throw new ContractViolationException($"Expected a mapping for {context} but got '{obj?.GetType().Name ?? "null"}'.");

    // H4: exposed as internal for testing; supports REPO_ROOT env var and .reporoot marker anchor
    internal static string ResolveSpecPath(string spec, string? startDir = null)
    {
        var root = Environment.GetEnvironmentVariable("REPO_ROOT");
        if (!string.IsNullOrWhiteSpace(root))
            return Path.Combine(root, spec.Replace('/', Path.DirectorySeparatorChar));

        var dir = new DirectoryInfo(startDir ?? AppContext.BaseDirectory);
        while (dir is not null)
        {
            if (File.Exists(Path.Combine(dir.FullName, ".reporoot")))
                return Path.Combine(dir.FullName, spec.Replace('/', Path.DirectorySeparatorChar));
            dir = dir.Parent;
        }

        throw new InvalidOperationException(
            "Cannot locate repository root (no .reporoot marker found in ancestor directories). Set REPO_ROOT to override.");
    }
}
