using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using AIKnowledgeAssistant.Api.Contracts.Documents;
using AIKnowledgeAssistant.Domain.Documents;
using Xunit;

namespace AIKnowledgeAssistant.IntegrationTests;

public sealed class DocumentIngestionEndpointsTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public DocumentIngestionEndpointsTests(CustomWebApplicationFactory factory)
    {
        if (CustomWebApplicationFactory.PostgresTestsEnabled)
        {
            factory.EnsureDatabaseMigrated();
        }

        _client = factory.CreateClient();
    }

    [PostgresFact]
    public async Task RunIngestion_UploadedDocument_BecomesReadyWithMetadata()
    {
        var upload = await UploadAsync($"ingest-{Guid.NewGuid():N}.txt", "hello");
        Assert.Equal(HttpStatusCode.Created, upload.StatusCode);
        var created = await upload.Content.ReadFromJsonAsync<DocumentDetailDto>();
        Assert.NotNull(created);
        Assert.Equal(DocumentStatus.Uploaded, created.Status);
        Assert.Equal(0, created.Ingestion.AttemptCount);

        var runResponse = await _client.PostAsync($"/api/documents/{created.Id}/ingestion/run", null);
        Assert.Equal(HttpStatusCode.OK, runResponse.StatusCode);

        var ingested = await runResponse.Content.ReadFromJsonAsync<DocumentDetailDto>();
        Assert.NotNull(ingested);
        Assert.Equal(DocumentStatus.Ready, ingested.Status);
        Assert.Equal(1, ingested.Ingestion.AttemptCount);
        Assert.NotNull(ingested.Ingestion.StartedAtUtc);
        Assert.NotNull(ingested.Ingestion.CompletedAtUtc);

        var chunksResponse = await _client.GetAsync($"/api/documents/{created.Id}/chunks");
        Assert.Equal(HttpStatusCode.OK, chunksResponse.StatusCode);
        var chunkList = await chunksResponse.Content.ReadFromJsonAsync<List<DocumentChunkDto>>();
        Assert.NotNull(chunkList);
        Assert.NotEmpty(chunkList);
        Assert.Equal(0, chunkList[0].ChunkIndex);
        Assert.True(chunkList[0].EndOffset > chunkList[0].StartOffset);
    }

    [PostgresFact]
    public async Task RunIngestion_WhenAlreadyReady_ReturnsConflict()
    {
        var upload = await UploadAsync($"ready-{Guid.NewGuid():N}.txt", "hello");
        var created = await upload.Content.ReadFromJsonAsync<DocumentDetailDto>();
        Assert.NotNull(created);

        var first = await _client.PostAsync($"/api/documents/{created.Id}/ingestion/run", null);
        Assert.Equal(HttpStatusCode.OK, first.StatusCode);

        var second = await _client.PostAsync($"/api/documents/{created.Id}/ingestion/run", null);
        Assert.Equal(HttpStatusCode.Conflict, second.StatusCode);
    }

    private async Task<HttpResponseMessage> UploadAsync(string fileName, string text)
    {
        using var content = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(Encoding.UTF8.GetBytes(text));
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("text/plain");
        content.Add(fileContent, "file", fileName);
        return await _client.PostAsync("/api/documents", content);
    }
}
