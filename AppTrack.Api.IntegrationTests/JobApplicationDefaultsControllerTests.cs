using AppTrack.Api.IntegrationTests.Seeddata;
using AppTrack.Api.IntegrationTests.Seeddata.JobApplicationDefaults;
using AppTrack.Api.IntegrationTests.Seeddata.User;
using AppTrack.Api.Models;
using AppTrack.Application.Features.JobApplicationDefaults.Commands.UpdateApplicationDefaults;
using AppTrack.Application.Features.JobApplicationDefaults.Dto;
using AppTrack.Application.Features.JobApplicationDefaults.Queries.GetJobApplicationDefaultsByUserId;
using Shouldly;
using System.Net;
using System.Net.Http.Json;

namespace AppTrack.Api.IntegrationTests;

public class UpdateJobApplicationDefaultsIntegrationTests : IClassFixture<FakeAuthWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly FakeAuthWebApplicationFactory _factory;

    public UpdateJobApplicationDefaultsIntegrationTests(FakeAuthWebApplicationFactory factory)
    {
        _client = factory.CreateAuthenticatedClient();
        this._factory = factory;
    }

    [Fact]
    public async Task UpdateJobApplicationDefaults_ShouldReturn204_WhenCommandIsValid()
    {
        // Arrange
        var (userId, defaultsId) = await SeedHelper.CreateUserWithJobDefaultsAsync(_factory.Services);
        var command = new UpdateJobApplicationDefaultsCommand
        {
            Id = defaultsId,
            UserId = userId,
            Name = "Updated Name",
            Position = "Updated Position",
            Location = "Updated Location"
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/JobApplicationsDefaults/{command.Id}", command);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task UpdateJobApplicationDefaults_ShouldReturn400_WhenIdIsInvalid()
    {
        // Arrange
        var userId = await ApplicationUserSeedHelper.CreateTestUserAsync(_factory.Services);
        var command = new UpdateJobApplicationDefaultsCommand
        {
            Id = 0,
            UserId = userId,
            Name = "Valid",
            Position = "Valid",
            Location = "Valid"
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/JobApplicationsDefaults/{command.Id}", command);

        // Assert
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
        //Arrange
        var (_, defaultsId) = await SeedHelper.CreateUserWithJobDefaultsAsync(_factory.Services);
        var command = new UpdateJobApplicationDefaultsCommand
        {
            Id = defaultsId,
            UserId = "invalid!user",
            Name = "Valid",
            Position = "Valid",
            Location = "Valid"
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/JobApplicationsDefaults/{command.Id}", command);

        // Assert
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
        //Arrange
        var (userId, defaultsId) = await SeedHelper.CreateUserWithJobDefaultsAsync(_factory.Services);
        var longValue = new string('x', 51);
        var command = new UpdateJobApplicationDefaultsCommand
        {
            Id = defaultsId,
            UserId = userId,
            Name = propertyName == "Name" ? longValue : "Valid",
            Position = propertyName == "Position" ? longValue : "Valid",
            Location = propertyName == "Location" ? longValue : "Valid"
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/JobApplicationsDefaults/{command.Id}", command);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        var problem = await response.Content.ReadFromJsonAsync<CustomProblemDetails>();
        problem.ShouldNotBeNull();

        problem.Errors.ShouldContainKey(propertyName);
        problem.Errors[propertyName].Any(msg => msg.Contains("must not exceed 50 characters")).ShouldBeTrue();
    }

    [Fact]
    public async Task UpdateJobApplicationDefaults_ShouldReturn400_WhenEntityDoesNotExist()
    {
        // Arrange
        var userId = await ApplicationUserSeedHelper.CreateTestUserAsync(_factory.Services);
        var command = new UpdateJobApplicationDefaultsCommand
        {
            Id = 9999, 
            UserId = userId,
            Name = "Valid",
            Position = "Valid",
            Location = "Valid"
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/JobApplicationsDefaults/{command.Id}", command);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        var problem = await response.Content.ReadFromJsonAsync<CustomProblemDetails>();
        problem.ShouldNotBeNull();
        problem.Errors.ShouldContainKey("Id");
        problem.Errors["Id"].Any(msg => msg.Contains("not found")).ShouldBeTrue();
    }

    [Fact]
    public async Task UpdateJobApplicationDefaults_ShouldReturn400_WhenEntityBelongsToAnotherUser()
    {
        // Arrange
        var randomUserId = await ApplicationUserSeedHelper.CreateTestUserAsync(_factory.Services);
        var (_, defaultsId) = await SeedHelper.CreateUserWithJobDefaultsAsync(_factory.Services);
        var command = new UpdateJobApplicationDefaultsCommand
        {
            Id = defaultsId,
            UserId = randomUserId, 
            Name = "Valid",
            Position = "Valid",
            Location = "Valid"
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/JobApplicationsDefaults/{command.Id}", command);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        var problem = await response.Content.ReadFromJsonAsync<CustomProblemDetails>();
        problem.ShouldNotBeNull();
        problem.Errors.ShouldContainKey("UserId");
        problem.Errors["UserId"].Any(msg => msg.Contains("not assigned to this user")).ShouldBeTrue();
    }

    [Fact]
    public async Task GetJobApplicationDefaults_ShouldCreateDefaultsSettings_WhenNotExisting()
    {
        var command = new GetJobApplicationDefaultsByUserIdQuery
        {
            UserId = Guid.NewGuid().ToString(), // random user
        };

        var response = await _client.GetAsync($"/api/JobApplicationsDefaults/{command.UserId}");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<JobApplicationDefaultsDto>();
        result.ShouldNotBeNull();
        result.UserId.ShouldBe(command.UserId);
    }

    [Fact]
    public async Task GetJobApplicationDefaults_ShouldReturn400_WhenUserIdIsInvalid()
    {
        var command = new GetJobApplicationDefaultsByUserIdQuery
        {
            UserId = "invalidUerId!#"
        };

        var response = await _client.GetAsync($"/api/JobApplicationsDefaults/{command.UserId}");

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        var problem = await response.Content.ReadFromJsonAsync<CustomProblemDetails>();
        problem.ShouldNotBeNull();
        problem.Errors.ShouldContainKey("UserId");
        problem.Errors["UserId"].Any(msg => msg.Contains("UserId contains invalid characters.")).ShouldBeTrue();
    }

    [Fact]
    public async Task GetJobApplicationDefaults_ShouldReturn404_WhenUserIdIsEmpty()
    {
        var command = new GetJobApplicationDefaultsByUserIdQuery
        {
            UserId = ""
        };

        var response = await _client.GetAsync($"/api/JobApplicationsDefaults/{command.UserId}");

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }
}
