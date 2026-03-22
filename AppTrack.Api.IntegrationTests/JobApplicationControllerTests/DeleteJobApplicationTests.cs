using AppTrack.Api.IntegrationTests.Seeddata;
using Shouldly;
using System.Net;

namespace AppTrack.Api.IntegrationTests.JobApplicationControllerTests;

public class DeleteJobApplicationTests : IClassFixture<FakeAuthWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly FakeAuthWebApplicationFactory _factory;

    public DeleteJobApplicationTests(FakeAuthWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateAuthenticatedClient();
    }

    [Fact]
    public async Task DeleteJobApplication_ShouldReturn204_WhenCommandIsValid()
    {
        // Arrange
        var jobApplicationId = await SeedHelper.CreateJobApplicationForTestUserAsync(_factory.Services);

        // Act
        var response = await _client.DeleteAsync($"/api/job-applications/{jobApplicationId}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }
}
