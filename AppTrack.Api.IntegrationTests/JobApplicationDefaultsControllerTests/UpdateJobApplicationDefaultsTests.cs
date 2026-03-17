using AppTrack.Api.IntegrationTests.Auth;
using AppTrack.Api.IntegrationTests.Seeddata;
using AppTrack.Application.Features.JobApplicationDefaults.Commands.UpdateApplicationDefaults;
using AppTrack.Application.Features.JobApplicationDefaults.Dto;
using Shouldly;
using System.Net;
using System.Net.Http.Json;

namespace AppTrack.Api.IntegrationTests.JobApplicationDefaultsControllerTests;

public class UpdateJobApplicationDefaultsTests : IClassFixture<FakeAuthWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly FakeAuthWebApplicationFactory _factory;

    public UpdateJobApplicationDefaultsTests(FakeAuthWebApplicationFactory factory)
    {
        _client = factory.CreateAuthenticatedClient();
        this._factory = factory;
    }

    [Fact]
    public async Task UpdateJobApplicationDefaults_ShouldReturn200_WhenCommandIsValid()
    {
        // Arrange
        var defaultsId = await SeedHelper.CreateJobDefaultsForTestUserAsync(_factory.Services);
        var command = new UpdateJobApplicationDefaultsCommand
        {
            Id = defaultsId,
            UserId = TestAuthHandler.TestUserId,
            Name = "Updated Name",
            Position = "Updated Position",
            Location = "Updated Location"
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/job-application-defaults/{command.Id}", command);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<JobApplicationDefaultsDto>();
        result.ShouldNotBeNull();
        result.Name.ShouldBe(command.Name);
    }
}
