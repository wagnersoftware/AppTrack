
using AppTrack.Api.IntegrationTests.Auth;
using AppTrack.Api.IntegrationTests.Seeddata;
using AppTrack.Application.Features.JobApplications.Dto;
using Shouldly;
using System.Net;
using System.Net.Http.Json;

namespace AppTrack.Api.IntegrationTests.JobApplicationControllerTests;

public class GetAllJobApplicationsForUserTests : IClassFixture<FakeAuthWebApplicationFactory>
{
    private readonly FakeAuthWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public GetAllJobApplicationsForUserTests(FakeAuthWebApplicationFactory factory)
    {
        this._factory = factory;
        this._client = factory.CreateAuthenticatedClient();
    }

    [Fact]
    public async Task GetJobApplicationsForUser_ShouldReturn200_WhenJobApplicationsExist()
    {
        // Arrange
        var jobApplicationId = await SeedHelper.CreateJobApplicationForTestUserAsync(_factory.Services);

        // Act
        var response = await _client.GetAsync("api/users/job-applications");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var jobApplications = await response.Content.ReadFromJsonAsync<List<JobApplicationDto>>();
        jobApplications.ShouldNotBeNull();
        jobApplications.ShouldNotBeEmpty();
        jobApplications.All(x => x.UserId == TestAuthHandler.TestUserId).ShouldBeTrue();
        jobApplications.Any(x => x.Id == jobApplicationId).ShouldBeTrue();
    }
}
