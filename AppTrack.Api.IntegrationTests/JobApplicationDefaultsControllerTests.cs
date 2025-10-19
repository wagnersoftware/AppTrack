using AppTrack.Api.IntegrationTests.Seeddata.JobApplicationDefaults;
using AppTrack.Api.IntegrationTests.Seeddata.User;
using AppTrack.Api.Models;
using AppTrack.Application.Features.JobApplicationDefaults.Commands.UpdateApplicationDefaults;
using Shouldly;
using System.Net;
using System.Net.Http.Json;

namespace AppTrack.Api.IntegrationTests;

public class UpdateJobApplicationDefaultsIntegrationTests : IClassFixture<FakeAuthWebApplicationFactory>
{
    private readonly HttpClient _client;

    public UpdateJobApplicationDefaultsIntegrationTests(FakeAuthWebApplicationFactory factory)
    {
        _client = factory.CreateAuthenticatedClient();
    }

    [Fact]
    public async Task UpdateJobApplicationDefaults_ShouldReturn204_WhenCommandIsValid()
    {
        var command = new UpdateJobApplicationDefaultsCommand
        {
            Id = JobApplicationDefaultsSeed.JobApplicationDefaults1Id,
            UserId = ApplicationUserSeed.User1Id,
            Name = "Updated Name",
            Position = "Updated Position",
            Location = "Updated Location"
        };

        var response = await _client.PutAsJsonAsync($"/api/JobApplicationsDefaults/{command.Id}", command);

        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task UpdateJobApplicationDefaults_ShouldReturn400_WhenIdIsInvalid()
    {
        var command = new UpdateJobApplicationDefaultsCommand
        {
            Id = 0,
            UserId = ApplicationUserSeed.User1Id,
            Name = "Valid",
            Position = "Valid",
            Location = "Valid"
        };

        var response = await _client.PutAsJsonAsync($"/api/JobApplicationsDefaults/{command.Id}", command);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        var problem = await response.Content.ReadFromJsonAsync<CustomProblemDetails>();
        problem.ShouldNotBeNull();

        problem.Errors.ShouldContainKey("Id");
        problem.Errors["Id"].Any(msg => msg.Contains("must be greater than 0")).ShouldBeTrue();
        problem.Errors["Id"].Any(msg => msg.Contains("is required")).ShouldBeTrue();
        problem.Errors["Id"].Any(msg => msg.Contains("not found")).ShouldBeTrue();
    }

    [Fact]
    public async Task UpdateJobApplicationDefaults_ShouldReturn400_WhenUserIdIsInvalid()
    {
        var command = new UpdateJobApplicationDefaultsCommand
        {
            Id = JobApplicationDefaultsSeed.JobApplicationDefaults1Id,
            UserId = "invalid!user",
            Name = "Valid",
            Position = "Valid",
            Location = "Valid"
        };

        var response = await _client.PutAsJsonAsync($"/api/JobApplicationsDefaults/{command.Id}", command);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        var problem = await response.Content.ReadFromJsonAsync<CustomProblemDetails>();
        problem.ShouldNotBeNull();

        problem.Errors.ShouldContainKey("UserId");
        problem.Errors["UserId"].Any(msg => msg.Contains("invalid characters")).ShouldBeTrue();
    }

    [Theory]
    [InlineData("Name")]
    [InlineData("Position")]
    [InlineData("Location")]
    public async Task UpdateJobApplicationDefaults_ShouldReturn400_WhenFieldExceedsMaxLength(string propertyName)
    {
        var longValue = new string('x', 51);
        var command = new UpdateJobApplicationDefaultsCommand
        {
            Id = JobApplicationDefaultsSeed.JobApplicationDefaults1Id,
            UserId = ApplicationUserSeed.User1Id,
            Name = propertyName == "Name" ? longValue : "Valid",
            Position = propertyName == "Position" ? longValue : "Valid",
            Location = propertyName == "Location" ? longValue : "Valid"
        };

        var response = await _client.PutAsJsonAsync($"/api/JobApplicationsDefaults/{command.Id}", command);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        var problem = await response.Content.ReadFromJsonAsync<CustomProblemDetails>();
        problem.ShouldNotBeNull();

        problem.Errors.ShouldContainKey(propertyName);
        problem.Errors[propertyName].Any(msg => msg.Contains("must not exceed 50 characters")).ShouldBeTrue();
    }

    [Fact]
    public async Task UpdateJobApplicationDefaults_ShouldReturn400_WhenEntityDoesNotExist()
    {
        var command = new UpdateJobApplicationDefaultsCommand
        {
            Id = 9999, 
            UserId = ApplicationUserSeed.User1Id,
            Name = "Valid",
            Position = "Valid",
            Location = "Valid"
        };

        var response = await _client.PutAsJsonAsync($"/api/JobApplicationsDefaults/{command.Id}", command);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        var problem = await response.Content.ReadFromJsonAsync<CustomProblemDetails>();
        problem.ShouldNotBeNull();
        problem.Errors.ShouldContainKey("Id");
        problem.Errors["Id"].Any(msg => msg.Contains("not found")).ShouldBeTrue();
    }

    [Fact]
    public async Task UpdateJobApplicationDefaults_ShouldReturn400_WhenEntityBelongsToAnotherUser()
    {
        var command = new UpdateJobApplicationDefaultsCommand
        {
            Id = JobApplicationDefaultsSeed.JobApplicationDefaults2Id,
            UserId = ApplicationUserSeed.User1Id, 
            Name = "Valid",
            Position = "Valid",
            Location = "Valid"
        };

        var response = await _client.PutAsJsonAsync($"/api/JobApplicationsDefaults/{command.Id}", command);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        var problem = await response.Content.ReadFromJsonAsync<CustomProblemDetails>();
        problem.ShouldNotBeNull();
        problem.Errors.ShouldContainKey("UserId");
        problem.Errors["UserId"].Any(msg => msg.Contains("not assigned to this user")).ShouldBeTrue();
    }
}
