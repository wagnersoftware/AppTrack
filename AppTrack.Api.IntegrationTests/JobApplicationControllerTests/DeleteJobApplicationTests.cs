using AppTrack.Api.IntegrationTests.Seeddata;
using AppTrack.Api.Models;
using Shouldly;
using System.Net;
using System.Net.Http.Json;

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

    [Fact]
    public async Task DeleteJobApplication_ShouldReturnMethodNotAllowed_WhenIdIsMissing()
    {
        // Act
        var response = await _client.DeleteAsync("/api/job-applications/");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.MethodNotAllowed);
    }

    [Fact]
    public async Task DeleteJobApplication_ShouldReturn400_WhenJobApplicationDoesNotExist()
    {
        // Arrange
        var invalidJobApplicationId = 999;

        // Act
        var response = await _client.DeleteAsync($"/api/job-applications/{invalidJobApplicationId}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        var problem = await response.Content.ReadFromJsonAsync<CustomProblemDetails>();
        problem.ShouldNotBeNull();
        problem.Errors.ShouldContainKey("");
        problem.Errors[""].Any(msg => msg.Contains("doesn't exist")).ShouldBeTrue();
    }

    [Fact]
    public async Task DeleteJobApplication_ShouldReturn400_WhenJobApplicationBelongsToDifferentUser()
    {
        // Arrange – job application belongs to a random user, not the authenticated test user
        var (_, jobApplicationId) = await SeedHelper.CreateUserWithJobApplicationAsync(_factory.Services);

        // Act
        var response = await _client.DeleteAsync($"/api/job-applications/{jobApplicationId}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        var problem = await response.Content.ReadFromJsonAsync<CustomProblemDetails>();
        problem.ShouldNotBeNull();
        problem.Errors.ShouldContainKey("");
        problem.Errors[""].Any(msg => msg.Contains("doesn't exist")).ShouldBeTrue();
    }
}
