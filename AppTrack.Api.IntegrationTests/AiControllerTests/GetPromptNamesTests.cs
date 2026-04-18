using AppTrack.Api.IntegrationTests.Seeddata;
using AppTrack.Application.Features.ApplicationText.Dto;
using Shouldly;
using System.Net;
using System.Net.Http.Json;

namespace AppTrack.Api.IntegrationTests.AiControllerTests;

public class GetPromptNamesTests : IClassFixture<FakeAuthWebApplicationFactory>
{
    private readonly FakeAuthWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public GetPromptNamesTests(FakeAuthWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateAuthenticatedClient();
    }

    [Fact]
    public async Task GetPromptNames_ShouldReturn200_WhenAiSettingsExist()
    {
        await SeedHelper.CreateAiSettingsForTestUserAsync(_factory.Services);

        var response = await _client.GetAsync("/api/ai/prompt-names");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<GetPromptNamesDto>();
        result.ShouldNotBeNull();
        result.Names.ShouldNotBeNull();
        result.Names.ShouldContain("Default");
    }
}
