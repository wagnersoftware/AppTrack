using AppTrack.Api.IntegrationTests.Seeddata;
using AppTrack.Api.Models;
using AppTrack.Application.Features.JobApplicationDefaults.Dto;
using Shouldly;
using System.Net;
using System.Net.Http.Json;

namespace AppTrack.Api.IntegrationTests.UserControllerTests;

public class GetJobApplicationDefaultsForUserTests : IClassFixture<FakeAuthWebApplicationFactory>
{

    private readonly HttpClient _client;
    private readonly FakeAuthWebApplicationFactory _factory;

    public GetJobApplicationDefaultsForUserTests(FakeAuthWebApplicationFactory factory)
    {
        _client = factory.CreateAuthenticatedClient();
        this._factory = factory;
    }

    [Fact]
    public async Task GetJobApplicationDefaults_ShouldReturnDefaultsSettings_WhenExisting()
    {
        //Arrange
        var (userId, defaultsId) = await SeedHelper.CreateUserWithJobDefaultsAsync(_factory.Services);

        //act
        var response = await _client.GetAsync($"/api/users/{userId}/job-application-defaults/");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<JobApplicationDefaultsDto>();
        result.ShouldNotBeNull();
        result.UserId.ShouldBe(userId);
        result.Id.ShouldBe(defaultsId);
    }

    [Fact]
    public async Task GetJobApplicationDefaults_ShouldCreateDefaultsSettings_WhenNotExisting()
    {
        //Arrange
        var userId = Guid.NewGuid().ToString(); // random user

        //act
        var response = await _client.GetAsync($"/api/users/{userId}/job-application-defaults/");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<JobApplicationDefaultsDto>();
        result.ShouldNotBeNull();
        result.UserId.ShouldBe(userId);
    }

    [Fact]
    public async Task GetJobApplicationDefaults_ShouldReturn400_WhenUserIdIsInvalid()
    {
        // Arrange
        var userId = "invalidUserId!";

        // Act
        var response = await _client.GetAsync($"/api/users/{userId}/job-application-defaults/");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        var problem = await response.Content.ReadFromJsonAsync<CustomProblemDetails>();
        problem.ShouldNotBeNull();
        problem.Errors.ShouldContainKey("UserId");
        problem.Errors["UserId"].Any(msg => msg.Contains("UserId contains invalid characters.")).ShouldBeTrue();
    }

    [Fact]
    public async Task GetJobApplicationDefaults_ShouldReturn404_WhenUserIdIsEmpty()
    {
        // Arrange
        var userId = string.Empty;

        // Act
        var response = await _client.GetAsync($"/api/users/{userId}/job-application-defaults/");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }
}
