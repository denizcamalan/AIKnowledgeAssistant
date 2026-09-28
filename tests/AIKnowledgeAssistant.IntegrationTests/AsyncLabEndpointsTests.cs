using System.Net.Http.Json;
using AIKnowledgeAssistant.Api.Controllers;
using Microsoft.AspNetCore.Hosting;

namespace AIKnowledgeAssistant.IntegrationTests;

public sealed class AsyncLabEndpointsTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public AsyncLabEndpointsTests(CustomWebApplicationFactory factory)
    {
        _client = factory.WithWebHostBuilder(builder => builder.UseEnvironment("Development")).CreateClient();
    }

    [Fact]
    public async Task Get_ReturnsIoAndCpuSamples()
    {
        var response = await _client.GetAsync("/api/labs/async?ioDelayMs=1");
        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadFromJsonAsync<AsyncLabResponse>();
        Assert.NotNull(body);
        Assert.Equal("io-bound", body.IoBound.Kind);
        Assert.False(body.IoBound.OffloadedToThreadPool);
        Assert.Equal("cpu-bound", body.CpuBound.Kind);
        Assert.True(body.CpuBound.OffloadedToThreadPool);
        Assert.Contains(body.Notes, note => note.Contains("ConfigureAwait", StringComparison.Ordinal));
        Assert.Contains(body.Notes, note => note.Contains("Sync-over-async", StringComparison.Ordinal));
    }

    [Fact]
    public async Task Get_WhenClientCancelsDuringIoDelay_ThrowsOperationCanceledException()
    {
        using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(40));

        var exception = await Record.ExceptionAsync(() =>
            _client.GetAsync("/api/labs/async?ioDelayMs=400", cts.Token));

        Assert.IsAssignableFrom<OperationCanceledException>(exception);
    }
}
