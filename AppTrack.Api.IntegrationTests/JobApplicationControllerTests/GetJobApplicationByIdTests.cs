using AppTrack.Api.IntegrationTests.Auth;
using AppTrack.Api.IntegrationTests.Seeddata;
using AppTrack.Application.Features.JobApplications.Dto;
using Shouldly;
using System.Net;
using System.Net.Http.Json;

namespace AppTrack.Api.IntegrationTests.JobApplicationControllerTests;

public class GetJobApplicationByIdTests : IClassFixture<FakeAuthWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly FakeAuthWebApplicationFactory _factory;

    public GetJobApplicationByIdTests(FakeAuthWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateAuthenticatedClient();
    }

    [Fact]
    public async Task GetJobApplicationById_ShouldReturn200_WhenRequestIsValid()
    {
        var jobApplicationId = await SeedHelper.CreateJobApplicationForTestUserAsync(_factory.Services);

        var response = await _client.GetAsync($"/api/job-applications/{jobApplicationId}");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<JobApplicationDto>();
        result.ShouldNotBeNull();
        result.Id.ShouldBe(jobApplicationId);
        result.UserId.ShouldBe(TestAuthHandler.TestUserId);
    }
}
