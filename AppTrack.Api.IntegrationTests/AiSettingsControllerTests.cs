using AppTrack.Api.IntegrationTests;
using AppTrack.Api.Models;
using Shouldly;
using System.Net;
using System.Net.Http.Json;

namespace AppTrack.IntegrationTests;

public class AiSettingsControllerTests : IClassFixture<FakeAuthWebApplicationFactory>
{
    private readonly FakeAuthWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public AiSettingsControllerTests(FakeAuthWebApplicationFactory factory)
    {
        _factory = factory;
        _client = _factory.CreateAuthenticatedClient();
    }

    [Fact]
    public async Task GetUser_ShouldReturn404_WhenUserNotFound()
    {
        // Arrange
        var invalidUserId = "999";

        // Act
        var response = await _client.GetAsync($"/api/ai-settings/{invalidUserId}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);

        var problem = await response.Content.ReadFromJsonAsync<CustomProblemDetails>();
        problem?.Title.ShouldBe($"user {invalidUserId} not found");
    }
}
