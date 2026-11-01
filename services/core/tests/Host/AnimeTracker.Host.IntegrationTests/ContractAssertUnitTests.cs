using System.Net;
using AnimeTracker.Testing;
using Shouldly;

namespace AnimeTracker.Host.IntegrationTests;

/// <summary>
/// Unit tests for ContractAssert error branches — no WebApplicationFactory needed.
/// Uses absolute temp-file paths so spec resolution never touches REPO_ROOT or the real repo tree.
/// </summary>
public sealed class ContractAssertUnitTests : IDisposable
{
    private readonly string _tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));

    public ContractAssertUnitTests() => Directory.CreateDirectory(_tempDir);

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, recursive: true);
    }

    // Writes YAML to a temp file and returns its absolute path.
    // Path.Combine(repoRoot, absolutePath) on Windows returns absolutePath,
    // so the absolute path bypasses the root-anchor heuristic inside ResolveSpecPath.
    private string WriteSpec(string yaml)
    {
        var path = Path.Combine(_tempDir, $"spec-{Guid.NewGuid():N}.yaml");
        File.WriteAllText(path, yaml);
        return path;
    }

    private static HttpResponseMessage OkJson(string json) =>
        new(HttpStatusCode.OK)
        {
            Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json"),
        };

    // Branch 1: path not found in contract
    [Fact]
    public async Task SatisfiesAsync_path_not_found_throws()
    {
        var spec = WriteSpec("""
            openapi: '3.1.0'
            paths:
              /health:
                get:
                  operationId: getHealth
                  responses:
                    '200':
                      description: OK
            """);

        using var response = OkJson("{}");
        var ex = await Should.ThrowAsync<ContractViolationException>(
            () => ContractAssert.SatisfiesAsync(spec, "GET", "/nonexistent", response));

        ex.Message.ShouldContain("/nonexistent");
    }

    // Branch 2: method not found for the path
    [Fact]
    public async Task SatisfiesAsync_method_not_found_throws()
    {
        var spec = WriteSpec("""
            openapi: '3.1.0'
            paths:
              /health:
                get:
                  operationId: getHealth
                  responses:
                    '200':
                      description: OK
            """);

        using var response = OkJson("{}");
        var ex = await Should.ThrowAsync<ContractViolationException>(
            () => ContractAssert.SatisfiesAsync(spec, "DELETE", "/health", response));

        ex.Message.ShouldContain("DELETE");
    }

    // Branch 3: no 'responses' section for the operation
    [Fact]
    public async Task SatisfiesAsync_no_responses_section_throws()
    {
        var spec = WriteSpec("""
            openapi: '3.1.0'
            paths:
              /health:
                get:
                  operationId: getHealth
            """);

        using var response = OkJson("{}");
        var ex = await Should.ThrowAsync<ContractViolationException>(
            () => ContractAssert.SatisfiesAsync(spec, "GET", "/health", response));

        ex.Message.ShouldContain("responses");
    }

    // Branch 4: status code not declared in responses
    [Fact]
    public async Task SatisfiesAsync_status_code_not_declared_throws()
    {
        var spec = WriteSpec("""
            openapi: '3.1.0'
            paths:
              /health:
                get:
                  operationId: getHealth
                  responses:
                    '200':
                      description: OK
            """);

        using var response = new HttpResponseMessage(HttpStatusCode.NotFound)
        {
            Content = new StringContent("{}", System.Text.Encoding.UTF8, "application/json"),
        };
        var ex = await Should.ThrowAsync<ContractViolationException>(
            () => ContractAssert.SatisfiesAsync(spec, "GET", "/health", response));

        ex.Message.ShouldContain("404");
    }

    // Branch 5: response has a body but contract declares no 'content' (H3)
    [Fact]
    public async Task SatisfiesAsync_body_but_no_content_declared_throws()
    {
        var spec = WriteSpec("""
            openapi: '3.1.0'
            paths:
              /health:
                get:
                  operationId: getHealth
                  responses:
                    '200':
                      description: OK
            """);

        using var response = OkJson("{\"status\":\"ok\"}");
        var ex = await Should.ThrowAsync<ContractViolationException>(
            () => ContractAssert.SatisfiesAsync(spec, "GET", "/health", response));

        ex.Message.ShouldContain("no 'content'");
    }

    // Branch 5b: content-type not declared in the content map
    [Fact]
    public async Task SatisfiesAsync_content_type_not_declared_throws()
    {
        var spec = WriteSpec("""
            openapi: '3.1.0'
            paths:
              /health:
                get:
                  operationId: getHealth
                  responses:
                    '200':
                      description: OK
                      content:
                        application/json:
                          schema:
                            type: object
            """);

        using var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("<xml/>", System.Text.Encoding.UTF8, "text/xml"),
        };
        var ex = await Should.ThrowAsync<ContractViolationException>(
            () => ContractAssert.SatisfiesAsync(spec, "GET", "/health", response));

        ex.Message.ShouldContain("text/xml");
    }

    // Branch 6: $ref resolution failure
    [Fact]
    public async Task SatisfiesAsync_ref_resolution_failure_throws()
    {
        var spec = WriteSpec("""
            openapi: '3.1.0'
            paths:
              /health:
                get:
                  operationId: getHealth
                  responses:
                    '200':
                      description: OK
                      content:
                        application/json:
                          schema:
                            $ref: '#/components/schemas/NonExistent'
            """);

        using var response = OkJson("{\"status\":\"ok\"}");
        var ex = await Should.ThrowAsync<ContractViolationException>(
            () => ContractAssert.SatisfiesAsync(spec, "GET", "/health", response));

        ex.Message.ShouldContain("$ref");
    }

    // Branch 7: repository root not found (tested via internal ResolveSpecPath with a startDir
    // that has no AnimeTracker.sln in ancestor directories)
    [Fact]
    public void ResolveSpecPath_root_not_found_throws()
    {
        var saved = Environment.GetEnvironmentVariable("REPO_ROOT");
        try
        {
            Environment.SetEnvironmentVariable("REPO_ROOT", null);
            var ex = Should.Throw<InvalidOperationException>(
                () => ContractAssert.ResolveSpecPath(
                    "contracts/internal/core.v1.yaml",
                    startDir: Path.GetTempPath()));

            ex.Message.ShouldContain(".reporoot");
        }
        finally
        {
            Environment.SetEnvironmentVariable("REPO_ROOT", saved);
        }
    }
}
