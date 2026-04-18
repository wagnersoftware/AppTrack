using AppTrack.Api.IntegrationTests.Seeddata;
using AppTrack.Application.Features.ApplicationText.Dto;
using AppTrack.Application.Features.ApplicationText.Query.RenderPromptQuery;
using Shouldly;
using System.Net;
using System.Net.Http.Json;

namespace AppTrack.Api.IntegrationTests.AiControllerTests;

public class RenderPromptTests : IClassFixture<FakeAuthWebApplicationFactory>
{
    private readonly FakeAuthWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public RenderPromptTests(FakeAuthWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateAuthenticatedClient();
    }

    [Fact]
    public async Task RenderPrompt_ShouldReturn200_WhenRequestIsValid()
    {
        // AiSettingsSeedsHelper seeds AI settings with prompt "Default" + parameters "name"/"rate"
        await SeedHelper.CreateAiSettingsForTestUserAsync(_factory.Services);
        var jobApplicationId = await SeedHelper.CreateJobApplicationForTestUserAsync(_factory.Services);

        var request = new RenderPromptQuery
        {
            JobApplicationId = jobApplicationId,
            PromptKey = "Default",
        };

        var response = await _client.PostAsJsonAsync("/api/ai/render-prompt", request);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<RenderedPromptDto>();
        result.ShouldNotBeNull();
        result.Prompt.ShouldNotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task RenderPrompt_ShouldReturn400_WhenJobApplicationIdIsZero()
    {
        var request = new RenderPromptQuery
        {
            JobApplicationId = 0,
            PromptKey = "Default",
        };

        var response = await _client.PostAsJsonAsync("/api/ai/render-prompt", request);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }
}
