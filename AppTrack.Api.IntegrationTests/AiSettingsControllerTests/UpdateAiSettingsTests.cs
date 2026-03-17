using AppTrack.Api.IntegrationTests.Auth;
using AppTrack.Api.IntegrationTests.Seeddata;
using AppTrack.Application.Features.AiSettings.Commands.UpdateAiSettings;
using AppTrack.Application.Features.AiSettings.Dto;
using AppTrack.Application.Features.ApplicationText.Dto;
using Shouldly;
using System;
using System.Net;
using System.Net.Http.Json;

namespace AppTrack.Api.IntegrationTests.AiSettingsControllerTests;

public class UpdateAiSettingsTests : IClassFixture<FakeAuthWebApplicationFactory>
{
    private readonly FakeAuthWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public UpdateAiSettingsTests(FakeAuthWebApplicationFactory factory)
    {
        _factory = factory;
        _client = _factory.CreateAuthenticatedClient();
    }

    [Fact]
    public async Task UpdateAiSettings_ShouldReturn200_WhenRequestIsValid()
    {
        // Arrange
        var aiSettingsId = await SeedHelper.CreateAiSettingsForTestUserAsync(_factory.Services);
        var validRequest = new UpdateAiSettingsCommand
        {
            Id = aiSettingsId,
            UserId = TestAuthHandler.TestUserId,
            PromptParameter = new List<PromptParameterDto>
                {
                    new() { Key = "Temperature", Value = "0.8" },
                    new() { Key = "MaxTokens", Value = "1000" }
                }
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/ai-settings/{validRequest.Id}", validRequest);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<AiSettingsDto>();
        result.ShouldNotBeNull();
    }

    [Fact]
    public async Task GetChatModels_ShouldReturn200()
    {
        // Arrange
        await SeedHelper.CreateChatModels(_factory.Services);

        // Act
        var response = await _client.GetAsync($"/api/ai-settings/chat-models");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var chatModels = await response.Content.ReadFromJsonAsync<List<ChatModelDto>>();
        chatModels.ShouldNotBeNull();
        chatModels.ShouldNotBeEmpty();
        chatModels.All(x => !String.IsNullOrEmpty(x.ApiModelName)).ShouldBeTrue();
        chatModels.All(x => !String.IsNullOrEmpty(x.Description)).ShouldBeTrue();
        chatModels.All(x => !String.IsNullOrEmpty(x.Name)).ShouldBeTrue();
    }
}
