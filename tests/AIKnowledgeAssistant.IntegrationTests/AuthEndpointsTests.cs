using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using AIKnowledgeAssistant.Api.Contracts.Auth;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace AIKnowledgeAssistant.IntegrationTests;

public sealed class AuthEndpointsTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public AuthEndpointsTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.WithWebHostBuilder(builder => builder.UseEnvironment("Development")).CreateClient();
    }

    [Fact]
    public async Task GetAccountMe_WithoutToken_ReturnsUnauthorized()
    {
        var response = await _client.GetAsync("/api/account/me");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsTokenAndProfile()
    {
        var loginResponse = await _client.PostAsJsonAsync(
            "/api/auth/login",
            new LoginRequest { Email = "user@demo.local", Password = "User123!" });

        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);
        var login = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();
        Assert.NotNull(login);
        Assert.False(string.IsNullOrWhiteSpace(login.AccessToken));
        Assert.Contains("User", login.User.Roles);

        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/account/me");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", login.AccessToken);
        var meResponse = await _client.SendAsync(request);
        Assert.Equal(HttpStatusCode.OK, meResponse.StatusCode);

        var profile = await meResponse.Content.ReadFromJsonAsync<UserProfileDto>();
        Assert.NotNull(profile);
        Assert.Equal("user@demo.local", profile.Email);
    }

    [Fact]
    public async Task Login_WithInvalidPassword_ReturnsUnauthorizedProblemDetails()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/auth/login",
            new LoginRequest { Email = "user@demo.local", Password = "wrong" });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

}
