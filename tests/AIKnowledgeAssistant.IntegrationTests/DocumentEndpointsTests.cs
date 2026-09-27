using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using AIKnowledgeAssistant.Api.Contracts.Documents;
using Xunit;

namespace AIKnowledgeAssistant.IntegrationTests;

public sealed class DocumentEndpointsTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public DocumentEndpointsTests(CustomWebApplicationFactory factory)
    {
        if (CustomWebApplicationFactory.PostgresTestsEnabled)
        {
            factory.EnsureDatabaseMigrated();
        }

        _client = factory.CreateClient();
    }

    [PostgresFact]
    public async Task Upload_Get_Update_Delete_Flow_Works()
    {
        var fileName = $"flow-{Guid.NewGuid():N}.txt";
        var uploadResponse = await UploadAsync(fileName, "Sample content", "My display name");
        Assert.Equal(HttpStatusCode.Created, uploadResponse.StatusCode);
        Assert.NotNull(uploadResponse.Headers.Location);

        var created = await uploadResponse.Content.ReadFromJsonAsync<DocumentDetailDto>();
        Assert.NotNull(created);
        Assert.Equal("My display name", created.DisplayName);

        var getResponse = await _client.GetAsync($"/api/documents/{created.Id}");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var listResponse = await _client.GetAsync("/api/documents");
        Assert.Equal(HttpStatusCode.OK, listResponse.StatusCode);
        var list = await listResponse.Content.ReadFromJsonAsync<List<DocumentSummaryDto>>();
        Assert.NotNull(list);
        Assert.Contains(list, d => d.Id == created.Id);

        var updateResponse = await _client.PutAsJsonAsync(
            $"/api/documents/{created.Id}",
            new UpdateDocumentRequest { DisplayName = "Renamed" });
        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);

        var deleteResponse = await _client.DeleteAsync($"/api/documents/{created.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var missingResponse = await _client.GetAsync($"/api/documents/{created.Id}");
        Assert.Equal(HttpStatusCode.NotFound, missingResponse.StatusCode);
    }

    [PostgresFact]
    public async Task Upload_DuplicateFileName_ReturnsConflict()
    {
        var fileName = $"dup-{Guid.NewGuid():N}.txt";
        var first = await UploadAsync(fileName, "first");
        Assert.Equal(HttpStatusCode.Created, first.StatusCode);

        var second = await UploadAsync(fileName, "second");
        Assert.Equal(HttpStatusCode.Conflict, second.StatusCode);
    }

    [Fact]
    public async Task Upload_InvalidExtension_ReturnsBadRequest()
    {
        var response = await UploadAsync($"bad-{Guid.NewGuid():N}.exe", "data");
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Upload_WithoutFile_ReturnsBadRequest()
    {
        using var content = new MultipartFormDataContent();
        var response = await _client.PostAsync("/api/documents", content);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    private async Task<HttpResponseMessage> UploadAsync(string fileName, string text, string? displayName = null)
    {
        using var content = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(Encoding.UTF8.GetBytes(text));
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("text/plain");
        content.Add(fileContent, "file", fileName);

        if (displayName is not null)
        {
            content.Add(new StringContent(displayName), "displayName");
        }

        return await _client.PostAsync("/api/documents", content);
    }
}
