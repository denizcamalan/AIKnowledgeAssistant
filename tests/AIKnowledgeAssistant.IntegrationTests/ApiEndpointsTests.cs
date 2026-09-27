using System.Net;
using System.Net.Http.Json;
using AIKnowledgeAssistant.Api.Controllers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace AIKnowledgeAssistant.IntegrationTests;

public sealed class ApiEndpointsTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public ApiEndpointsTests(WebApplicationFactory<Program> factory) => _factory = factory;

    [Fact]
    public async Task GetHealth_ReturnsOkWithHealthyStatus()
    {
        using var client = _factory.CreateClient();
        var response = await client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<HealthResponse>();
        Assert.NotNull(body);
        Assert.Equal("healthy", body.Status);
    }

    [Fact]
    public async Task PostHealth_ReturnsMethodNotAllowed()
    {
        using var client = _factory.CreateClient();
        var response = await client.PostAsync("/health", null);

        Assert.Equal(HttpStatusCode.MethodNotAllowed, response.StatusCode);
    }

    [Fact]
    public async Task GetInfo_InDevelopment_UsesDevelopmentConfiguration()
    {
        using var factory = _factory.WithWebHostBuilder(builder => builder.UseEnvironment("Development"));
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/info");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<ApiInfoResponse>();
        Assert.NotNull(body);
        Assert.Equal("AI Knowledge Assistant (Development)", body.DisplayName);
        Assert.Equal("Development", body.EnvironmentName);
    }
}
