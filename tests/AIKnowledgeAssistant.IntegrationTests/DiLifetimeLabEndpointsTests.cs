using System.Net.Http.Json;
using AIKnowledgeAssistant.Api.Controllers;
using Microsoft.AspNetCore.Hosting;

namespace AIKnowledgeAssistant.IntegrationTests;

public sealed class DiLifetimeLabEndpointsTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public DiLifetimeLabEndpointsTests(CustomWebApplicationFactory factory)
    {
        _client = factory.WithWebHostBuilder(builder => builder.UseEnvironment("Development")).CreateClient();
    }

    [Fact]
    public async Task GetSnapshot_TransientDiffers_ScopedAndSingletonMatchWithinRequest()
    {
        var response = await _client.GetAsync("/api/labs/di");
        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadFromJsonAsync<DiLifetimeLabResponse>();
        Assert.NotNull(body);

        Assert.True(body.Transient.MatchesExpectedLifetimeBehavior);
        Assert.True(body.Scoped.MatchesExpectedLifetimeBehavior);
        Assert.True(body.Singleton.MatchesExpectedLifetimeBehavior);
    }

    [Fact]
    public async Task GetSnapshot_AcrossRequests_ScopedChanges_SingletonStable()
    {
        var first = await GetSnapshotAsync();
        var second = await GetSnapshotAsync();

        Assert.NotEqual(first.RequestScopeId, second.RequestScopeId);
        Assert.NotEqual(first.Scoped.FirstResolutionId, second.Scoped.FirstResolutionId);
        Assert.Equal(first.Singleton.FirstResolutionId, second.Singleton.FirstResolutionId);
    }

    private async Task<DiLifetimeLabResponse> GetSnapshotAsync()
    {
        var response = await _client.GetAsync("/api/labs/di");
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<DiLifetimeLabResponse>())!;
    }
}
