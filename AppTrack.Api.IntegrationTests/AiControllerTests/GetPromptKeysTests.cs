using AppTrack.Api.IntegrationTests.Seeddata;
using AppTrack.Application.Features.ApplicationText.Dto;
using Shouldly;
using System.Net;
using System.Net.Http.Json;

namespace AppTrack.Api.IntegrationTests.AiControllerTests;

public class GetPromptKeysTests : IClassFixture<FakeAuthWebApplicationFactory>
{
    private readonly FakeAuthWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public GetPromptKeysTests(FakeAuthWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateAuthenticatedClient();
    }

    [Fact]
    public async Task GetPromptKeys_ShouldReturn200_WhenAiSettingsExist()
    {
        await SeedHelper.CreateAiSettingsForTestUserAsync(_factory.Services);

        var response = await _client.GetAsync("/api/ai/prompt-keys");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<GetPromptKeysDto>();
        result.ShouldNotBeNull();
        result.Keys.ShouldNotBeNull();
        result.Keys.ShouldContain("Default");
    }
}
