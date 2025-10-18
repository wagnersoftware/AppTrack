using AppTrack.Api.IntegrationTests;
using AppTrack.Api.IntegrationTests.Seeddata.User;
using AppTrack.Api.Models;
using AppTrack.Application.Features.AiSettings.Dto;
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
    public async Task GetUser_ShouldReturnAiSettings_WhenUserExists()
    {
        // Arrange
        var validUserId = ApplicationUserSeed.User1Id;
        // Act
        var response = await _client.GetAsync($"/api/ai-settings?UserId={validUserId}");
        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var aiSettings = await response.Content.ReadFromJsonAsync<AiSettingsDto>();
        aiSettings.ShouldNotBeNull();
        aiSettings.UserId.ShouldBe(validUserId);
    }

    [Fact]
    public async Task GetUser_ShouldReturn404_WhenUserNotFound()
    {
        // Arrange
        var invalidUserId = "999";
        // Act
        var response = await _client.GetAsync($"/api/ai-settings?UserId={invalidUserId}");
        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        var problem = await response.Content.ReadFromJsonAsync<CustomProblemDetails>();
        problem?.Title.ShouldBe($"user {invalidUserId} not found");
    }

    [Fact]
    public async Task GetAiSettings_ShouldReturn400_WhenUserIdIsEmpty()
    {
        // Arrange
        var emptyUserId = string.Empty;

        // Act
        var response = await _client.GetAsync($"/api/ai-settings?UserId={emptyUserId}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        var problem = await response.Content.ReadFromJsonAsync<CustomProblemDetails>();
        problem.ShouldNotBeNull();
        problem.Title.ShouldBe("One or more validation errors occurred.");

        problem.Errors.ShouldContainKey("UserId");
        problem.Errors["UserId"].ShouldContain("The UserId field is required.");
    }
}
