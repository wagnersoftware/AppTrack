using AppTrack.Api.IntegrationTests.Seeddata;
using Shouldly;
using System.Net;

namespace AppTrack.Api.IntegrationTests.AiControllerTests;

public class DeleteAiTextTests : IClassFixture<FakeAuthWebApplicationFactory>
{
    private readonly FakeAuthWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public DeleteAiTextTests(FakeAuthWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateAuthenticatedClient();
    }

    [Fact]
    public async Task DeleteAiText_ShouldReturn204_WhenEntryExistsAndBelongsToUser()
    {
        var jobApplicationId = await SeedHelper.CreateJobApplicationForTestUserAsync(_factory.Services);
        var aiTextId = await SeedHelper.CreateAiTextForJobApplicationAsync(_factory.Services, jobApplicationId);

        var response = await _client.DeleteAsync($"/api/ai/text/{aiTextId}");

        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task DeleteAiText_ShouldReturn400_WhenEntryDoesNotExist()
    {
        var response = await _client.DeleteAsync("/api/ai/text/999999");

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }
}
