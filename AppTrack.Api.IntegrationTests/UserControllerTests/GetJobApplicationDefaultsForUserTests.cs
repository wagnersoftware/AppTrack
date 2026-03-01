using AppTrack.Api.IntegrationTests.Auth;
using AppTrack.Api.IntegrationTests.Seeddata;
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
        await SeedHelper.CreateJobDefaultsForTestUserAsync(_factory.Services);

        //act
        var response = await _client.GetAsync("/api/users/job-application-defaults");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<JobApplicationDefaultsDto>();
        result.ShouldNotBeNull();
        result.UserId.ShouldBe(TestAuthHandler.TestUserId);
    }

    [Fact]
    public async Task GetJobApplicationDefaults_ShouldCreateDefaultsSettings_WhenNotExisting()
    {
        //act
        var response = await _client.GetAsync("/api/users/job-application-defaults");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<JobApplicationDefaultsDto>();
        result.ShouldNotBeNull();
        result.UserId.ShouldBe(TestAuthHandler.TestUserId);
    }

    [Fact]
    public async Task GetJobApplicationDefaults_ShouldReturn404_WhenUserIdIsEmpty()
    {
        // Act – empty segment in URL resolves to 404 (no matching route)
        var response = await _client.GetAsync("/api/users//job-application-defaults/");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }
}
