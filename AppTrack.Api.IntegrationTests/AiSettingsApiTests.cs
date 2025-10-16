using AppTrack.Api.IntegrationTests.WebApplicationFactory;
using AppTrack.Api.Models;
using Shouldly;
using System.Net;
using System.Net.Http.Json;

namespace AppTrack.IntegrationTests;

public class AiSettingsApiTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    public AiSettingsApiTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateAuthenticatedClient();
    }

    [Fact]
    public async Task GetUser_ShouldReturn404_WhenUserNotFound()
    {
        // Arrange
        var invalidUserId = "99";

        // Act
        var response = await _client.GetAsync($"/api/ai-settings/{invalidUserId}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);

        var problem = await response.Content.ReadFromJsonAsync<CustomProblemDetails>();
        problem?.Title.ShouldBe($"user {invalidUserId} not found");
    }
}
