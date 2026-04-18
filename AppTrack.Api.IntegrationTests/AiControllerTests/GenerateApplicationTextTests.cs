using AppTrack.Api.IntegrationTests.Seeddata;
using AppTrack.Api.IntegrationTests.WebApplicationFactory;
using AppTrack.Application.Features.AiSettings.Commands.GenerateAiText;
using AppTrack.Application.Features.JobApplications.Dto;
using Shouldly;
using System.Net;
using System.Net.Http.Json;

namespace AppTrack.Api.IntegrationTests.AiControllerTests;

/// <summary>
/// Tests validation failure paths — no AI text generator stub required.
/// </summary>
public class GenerateAiTextValidationTests : IClassFixture<FakeAuthWebApplicationFactory>
{
    private readonly FakeAuthWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public GenerateAiTextValidationTests(FakeAuthWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateAuthenticatedClient();
    }

    [Fact]
    public async Task GenerateAiText_ShouldReturn400_WhenPromptIsEmpty()
    {
        var jobApplicationId = await SeedHelper.CreateJobApplicationForTestUserAsync(_factory.Services);

        var request = new GenerateAiTextCommand
        {
            JobApplicationId = jobApplicationId,
            Prompt = string.Empty,
            PromptKey = "cover-letter",
        };

        var response = await _client.PostAsJsonAsync("/api/ai/generate", request);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GenerateAiText_ShouldReturn400_WhenJobApplicationDoesNotExist()
    {
        var request = new GenerateAiTextCommand
        {
            JobApplicationId = 999999,
            Prompt = "Write a cover letter",
            PromptKey = "cover-letter",
        };

        var response = await _client.PostAsJsonAsync("/api/ai/generate", request);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }
}

/// <summary>
/// Tests the happy path with a stubbed AI text generator to avoid real OpenAI calls.
/// </summary>
public class GenerateAiTextHappyPathTests : IClassFixture<FakeAiTextWebApplicationFactory>
{
    private readonly FakeAiTextWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public GenerateAiTextHappyPathTests(FakeAiTextWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateAuthenticatedClient();
    }

    [Fact]
    public async Task GenerateAiText_ShouldReturn200_WhenRequestIsValid()
    {
        var jobApplicationId = await SeedHelper.CreateJobApplicationForTestUserAsync(_factory.Services);
        await SeedHelper.CreateAiSettingsWithChatModelForTestUserAsync(_factory.Services);

        var request = new GenerateAiTextCommand
        {
            JobApplicationId = jobApplicationId,
            Prompt = "Write a cover letter for the .NET Developer position",
            PromptKey = "cover-letter",
        };

        var response = await _client.PostAsJsonAsync("/api/ai/generate", request);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<GeneratedAiTextDto>();
        result.ShouldNotBeNull();
        result.GeneratedText.ShouldBe(StubAiTextGenerator.FakeGeneratedText);
    }

    [Fact]
    public async Task GenerateAiText_ShouldStoreEntryInAiTextHistory_WhenRequestIsValid()
    {
        var jobApplicationId = await SeedHelper.CreateJobApplicationForTestUserAsync(_factory.Services);
        await SeedHelper.CreateAiSettingsWithChatModelForTestUserAsync(_factory.Services);

        var request = new GenerateAiTextCommand
        {
            JobApplicationId = jobApplicationId,
            Prompt = "Write a cover letter",
            PromptKey = "history-test-key",
        };

        await _client.PostAsJsonAsync("/api/ai/generate", request);

        // Verify the history entry was persisted by fetching the job application
        var jobApplicationResponse = await _client.GetAsync($"/api/job-applications/{jobApplicationId}");
        jobApplicationResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

        var jobApplication = await jobApplicationResponse.Content.ReadFromJsonAsync<JobApplicationDto>();
        jobApplication.ShouldNotBeNull();
        jobApplication.AiTextHistory.ShouldContain(h =>
            h.PromptKey == "history-test-key" &&
            h.GeneratedText == StubAiTextGenerator.FakeGeneratedText);
    }
}
