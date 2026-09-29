using System.Net;
using System.Net.Http.Json;
using AIKnowledgeAssistant.Api.Contracts.Auth;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace AIKnowledgeAssistant.IntegrationTests;

public sealed class SpaCorsTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public SpaCorsTests(CustomWebApplicationFactory factory)
    {
        _client = factory.WithWebHostBuilder(builder => builder.UseEnvironment("Development")).CreateClient();
    }

    [Fact]
    public async Task Preflight_FromSpaOrigin_AllowsAuthorizationHeader()
    {
        using var request = new HttpRequestMessage(HttpMethod.Options, "/api/account/me");
        request.Headers.Add("Origin", "http://localhost:5173");
        request.Headers.Add("Access-Control-Request-Method", "GET");
        request.Headers.Add("Access-Control-Request-Headers", "authorization");

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Equal("http://localhost:5173", response.Headers.GetValues("Access-Control-Allow-Origin").Single());
        var allowHeaders = string.Join(',', response.Headers.GetValues("Access-Control-Allow-Headers"));
        Assert.Contains("authorization", allowHeaders, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Login_FromSpaOrigin_ExposesAllowOrigin()
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/auth/login");
        request.Headers.Add("Origin", "http://localhost:5173");
        request.Content = JsonContent.Create(new LoginRequest
        {
            Email = "user@demo.local",
            Password = "wrong-password",
        });

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Equal("http://localhost:5173", response.Headers.GetValues("Access-Control-Allow-Origin").Single());
    }
}
