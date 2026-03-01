
using AppTrack.Api.IntegrationTests.Auth;
using AppTrack.Api.IntegrationTests.Seeddata;
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
    public async Task GetAiSettings_ShouldCreateAiSettings_WhenNotExisting()
    {
        // Act
        var response = await _client.GetAsync("/api/users/ai-settings");
        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var aiSettings = await response.Content.ReadFromJsonAsync<AiSettingsDto>();
        aiSettings.ShouldNotBeNull();
        aiSettings.UserId.ShouldBe(TestAuthHandler.TestUserId);
    }

    [Fact]
    public async Task GetAiSettings_ShouldReturnAiSettingsForUser_WhenAiSettingsExist()
    {
        // Arrange
        var aiSettingsId = await SeedHelper.CreateAiSettingsForTestUserAsync(_factory.Services);
        // Act
        var response = await _client.GetAsync("/api/users/ai-settings");
        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var aiSettings = await response.Content.ReadFromJsonAsync<AiSettingsDto>();
        aiSettings.ShouldNotBeNull();
        aiSettings.UserId.ShouldBe(TestAuthHandler.TestUserId);
        aiSettings.Id.ShouldBe(aiSettingsId);
    }

    [Fact]
    public async Task GetAiSettings_ShouldReturn404_WhenUserIdIsEmpty()
    {
        // Arrange – empty segment in URL resolves to 404 (no matching route)
        var response = await _client.GetAsync("/api/users//ai-settings");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }
}
