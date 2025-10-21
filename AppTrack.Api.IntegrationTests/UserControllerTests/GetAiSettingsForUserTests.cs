
using AppTrack.Api.IntegrationTests.Seeddata.User;
using AppTrack.Api.Models;
using AppTrack.Application.Features.AiSettings.Dto;
using Shouldly;
using System.Net;
using System.Net.Http.Json;

namespace AppTrack.Api.IntegrationTests.UserControllerTests;

public class GetAiSettingsForUserTests : IClassFixture<FakeAuthWebApplicationFactory>
{
    private readonly FakeAuthWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public GetAiSettingsForUserTests(FakeAuthWebApplicationFactory factory)
    {
        _factory = factory;
        _client = _factory.CreateAuthenticatedClient();
    }


    [Fact]
    public async Task GetAiSettings_ShouldReturnAiSettings_WhenUserExists()
    {
        // Arrange
        var validUserId = await ApplicationUserSeedHelper.CreateTestUserAsync(_factory.Services);
        // Act
        var response = await _client.GetAsync($"/api/users/{validUserId}/ai-settings");
        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var aiSettings = await response.Content.ReadFromJsonAsync<AiSettingsDto>();
        aiSettings.ShouldNotBeNull();
        aiSettings.UserId.ShouldBe(validUserId);
    }

    [Fact]
    public async Task GetAiSettings_ShouldReturn404_WhenUserNotFound()
    {
        // Arrange
        var invalidUserId = "999";
        // Act
        var response = await _client.GetAsync($"/api/users/{invalidUserId}/ai-settings");
        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        var problem = await response.Content.ReadFromJsonAsync<CustomProblemDetails>();
        problem?.Title.ShouldBe($"user {invalidUserId} not found");
    }

    [Fact]
    public async Task GetAiSettings_ShouldReturn404_WhenUserIdIsEmpty()
    {
        // Arrange
        var emptyUserId = string.Empty;

        // Act
        var response = await _client.GetAsync($"/api/users/{emptyUserId}/ai-settings");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }
}
