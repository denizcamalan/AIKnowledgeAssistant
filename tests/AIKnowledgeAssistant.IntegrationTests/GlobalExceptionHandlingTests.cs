using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace AIKnowledgeAssistant.IntegrationTests;

public sealed class GlobalExceptionHandlingTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public GlobalExceptionHandlingTests(CustomWebApplicationFactory factory) => _factory = factory;

    [PostgresFact]
    public async Task GetMissingDocument_ReturnsProblemDetailsWithTraceId()
    {
        _factory.EnsureDatabaseMigrated();
        using var client = _factory.CreateClient();
        var response = await client.GetAsync($"/api/documents/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        var problem = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("Not Found", problem.GetProperty("title").GetString());
        Assert.True(problem.TryGetProperty("traceId", out var traceId));
        Assert.False(string.IsNullOrWhiteSpace(traceId.GetString()));
    }

    [Fact]
    public async Task UnhandledException_InProduction_DoesNotLeakInternalMessage()
    {
        using var client = _factory
            .WithWebHostBuilder(builder => builder.UseEnvironment(Environments.Production))
            .CreateClient();

        var response = await client.GetAsync("/api/labs/errors/unhandled");

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain("Laboratory unhandled", body);
        Assert.Contains("traceId", body, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Upload_WithoutFile_ReturnsValidationProblemWithTraceId()
    {
        using var client = _factory.WithWebHostBuilder(builder => builder.UseEnvironment("Development")).CreateClient();
        using var content = new MultipartFormDataContent();
        var response = await client.PostAsync("/api/documents", content);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(problem.TryGetProperty("traceId", out _));
        Assert.True(problem.TryGetProperty("errors", out _));
    }
}
