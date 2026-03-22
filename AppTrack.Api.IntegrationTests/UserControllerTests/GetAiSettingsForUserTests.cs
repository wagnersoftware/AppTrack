
using AppTrack.Api.IntegrationTests.Auth;
using AppTrack.Api.IntegrationTests.Seeddata;
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
    public async Task GetAiSettings_ShouldReturnAiSettingsForUser_WhenAiSettingsExist()
    {
        // Arrange
        await SeedHelper.CreateAiSettingsForTestUserAsync(_factory.Services);
        // Act
        var response = await _client.GetAsync("/api/users/ai-settings");
        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var aiSettings = await response.Content.ReadFromJsonAsync<AiSettingsDto>();
        aiSettings.ShouldNotBeNull();
        aiSettings.UserId.ShouldBe(TestAuthHandler.TestUserId);
    }
}
