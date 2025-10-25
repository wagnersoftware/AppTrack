using AppTrack.Api.IntegrationTests.Seeddata;
using AppTrack.Api.Models;
using AppTrack.Application.Features.JobApplications.Dto;
using Shouldly;
using System.Net;
using System.Net.Http.Json;

namespace AppTrack.Api.IntegrationTests.JobApplicationControllerTests;

public class GetJobApplicationTests : IClassFixture<FakeAuthWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly FakeAuthWebApplicationFactory _factory;

    public GetJobApplicationTests(FakeAuthWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateAuthenticatedClient();
    }

    [Fact]
    public async Task GetJobApplicationById_ShouldReturn200_WhenRequestIsValid()
    {
        var (userId, jobApplicationId) = await SeedHelper.CreateUserWithJobApplicationAsync(_factory.Services);

        var response = await _client.GetAsync($"/api/job-applications/{jobApplicationId}?UserId={userId}");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<JobApplicationDto>();
        result.ShouldNotBeNull();
        result.Id.ShouldBe(jobApplicationId);
        result.UserId.ShouldBe(userId);
    }

    [Fact]
    public async Task GetJobApplicationById_ShouldReturn400_WhenIdIsMissingOrZero()
    {
        var userId = (await SeedHelper.CreateUserAsync(_factory.Services));

        var response = await _client.GetAsync($"/api/job-applications/0?UserId={userId}");

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        var problem = await response.Content.ReadFromJsonAsync<CustomProblemDetails>();
        problem.ShouldNotBeNull();

        problem.Errors.ShouldContainKey("Id");
        problem.Errors["Id"].Any(msg => msg.Contains("is required")).ShouldBeTrue();
    }

    [Fact]
    public async Task GetJobApplicationById_ShouldReturn400_WhenUserIdIsMissing()
    {
        var (_, jobApplicationId) = await SeedHelper.CreateUserWithJobApplicationAsync(_factory.Services);

        var response = await _client.GetAsync($"/api/job-applications/{jobApplicationId}");

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        var problem = await response.Content.ReadFromJsonAsync<CustomProblemDetails>();
        problem.ShouldNotBeNull();

        problem.Errors.ShouldContainKey("userId");
        problem.Errors["userId"].Any(msg => msg.Contains("is required")).ShouldBeTrue();
    }

    [Fact]
    public async Task GetJobApplicationById_ShouldReturn400_WhenJobApplicationDoesNotExist()
    {
        var userId = (await SeedHelper.CreateUserAsync(_factory.Services));

        var invalidId = 99999;
        var response = await _client.GetAsync($"/api/job-applications/{invalidId}?UserId={userId}");

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        var problem = await response.Content.ReadFromJsonAsync<CustomProblemDetails>();
        problem.ShouldNotBeNull();

        problem.Errors.ShouldContainKey("Id");
        problem.Errors["Id"].Any(msg => msg.Contains("Job application not found.")).ShouldBeTrue();
    }

    [Fact]
    public async Task GetJobApplicationById_ShouldReturn400_WhenJobApplicationBelongsToAnotherUser()
    {
        var (_, jobApplicationId) = await SeedHelper.CreateUserWithJobApplicationAsync(_factory.Services);
        var user2 = await SeedHelper.CreateUserAsync(_factory.Services);

        var response = await _client.GetAsync($"/api/job-applications/{jobApplicationId}?UserId={user2}");

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        var problem = await response.Content.ReadFromJsonAsync<CustomProblemDetails>();
        problem.ShouldNotBeNull();

        problem.Errors.ShouldContainKey("UserId");
        problem.Errors["UserId"].Any(msg => msg.Contains("Job application doesn't belong to this user.")).ShouldBeTrue();
    }
}
