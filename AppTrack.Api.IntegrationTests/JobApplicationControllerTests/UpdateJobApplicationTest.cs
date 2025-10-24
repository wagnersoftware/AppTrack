using AppTrack.Api.IntegrationTests.Seeddata;
using AppTrack.Api.Models;
using AppTrack.Application.Features.JobApplications.Commands.UpdateJobApplication;
using AppTrack.Application.Features.JobApplications.Dto;
using AppTrack.Domain.Enums;
using Shouldly;
using System.Net;
using System.Net.Http.Json;

namespace AppTrack.Api.IntegrationTests.JobApplicationControllerTests;

public class UpdateJobApplicationTests : IClassFixture<FakeAuthWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly FakeAuthWebApplicationFactory _factory;

    public UpdateJobApplicationTests(FakeAuthWebApplicationFactory factory)
    {
        _client = factory.CreateAuthenticatedClient();
        this._factory = factory;
    }

    [Fact]
    public async Task UpdateJobApplication_ShouldReturn200_WhenCommandIsValid()
    {
        var (userId, jobApplicationId) = await SeedHelper.CreateUserWithJobApplicationAsync(_factory.Services);

        var command = new UpdateJobApplicationCommand
        {
            Id = jobApplicationId,
            UserId = userId,
            Name = "Updated Job",
            Position = "Senior Developer",
            URL = "https://company.com/job",
            JobDescription = "Updated description",
            Location = "Remote",
            ContactPerson = "Jane Doe",
            Status = JobApplicationStatus.New,
            StartDate = DateTime.UtcNow,
            DurationInMonths = "6"
        };

        var response = await _client.PutAsJsonAsync($"/api/job-applications/{command.Id}", command);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var updated = await response.Content.ReadFromJsonAsync<JobApplicationDto>();
        updated.ShouldNotBeNull();
        updated.Name.ShouldBe(command.Name);
    }

    [Theory]
    [InlineData("Name")]
    [InlineData("Position")]
    [InlineData("Location")]
    [InlineData("ContactPerson")]
    public async Task UpdateJobApplication_ShouldReturn400_WhenTextFieldExceedsMaxLength(string propertyName)
    {
        var (userId, jobApplicationId) = await SeedHelper.CreateUserWithJobApplicationAsync(_factory.Services);

        var longValue = new string('x', 201);
        var command = new UpdateJobApplicationCommand
        {
            Id = jobApplicationId,
            UserId = userId,
            Name = propertyName == "Name" ? longValue : "Valid",
            Position = propertyName == "Position" ? longValue : "Valid",
            URL = "https://company.com/job",
            JobDescription = "Valid",
            Location = propertyName == "Location" ? longValue : "Valid",
            ContactPerson = propertyName == "ContactPerson" ? longValue : "Valid",
            Status = JobApplicationStatus.New,
            StartDate = DateTime.UtcNow,
        };

        var response = await _client.PutAsJsonAsync($"/api/job-applications/{command.Id}", command);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        var problem = await response.Content.ReadFromJsonAsync<CustomProblemDetails>();
        problem.ShouldNotBeNull();
        problem.Errors.ShouldContainKey(propertyName);
        problem.Errors[propertyName].Any(msg => msg.Contains("must not exceed")).ShouldBeTrue();
    }

    [Fact]
    public async Task UpdateJobApplication_ShouldReturn400_WhenUrlIsInvalid()
    {
        var (userId, jobApplicationId) = await SeedHelper.CreateUserWithJobApplicationAsync(_factory.Services);

        var command = new UpdateJobApplicationCommand
        {
            Id = jobApplicationId,
            UserId = userId,
            Name = "Valid",
            Position = "Valid",
            URL = "invalid-url",
            JobDescription = "Valid",
            Location = "Valid",
            ContactPerson = "Valid",
            Status = JobApplicationStatus.New,
            StartDate = DateTime.UtcNow,
        };

        var response = await _client.PutAsJsonAsync($"/api/job-applications/{command.Id}", command);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        var problem = await response.Content.ReadFromJsonAsync<CustomProblemDetails>();
        problem.ShouldNotBeNull();
        problem.Errors.ShouldContainKey("URL");
        problem.Errors["URL"].Any(msg => msg.Contains("valid URL")).ShouldBeTrue();
    }

    [Fact]
    public async Task UpdateJobApplication_ShouldReturn400_WhenRequiredFieldsMissing()
    {
        var (_, jobApplicationId) = await SeedHelper.CreateUserWithJobApplicationAsync(_factory.Services);

        var command = new UpdateJobApplicationCommand { Id = jobApplicationId};

        var response = await _client.PutAsJsonAsync($"/api/job-applications/{command.Id}", command);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        var problem = await response.Content.ReadFromJsonAsync<CustomProblemDetails>();

        var requiredFields = new[]
        {
            "Name", "Position", "URL", "UserId", "JobDescription",
            "Location", "ContactPerson", "StartDate"
        };

        foreach (var field in requiredFields)
        {
            problem.ShouldNotBeNull();
            problem.Errors.ShouldContainKey(field);
            problem.Errors[field].Any(msg => msg.Contains("required")).ShouldBeTrue();
        }
    }

    [Fact]
    public async Task UpdateJobApplication_ShouldReturn400_WhenDurationInMonthsIsInvalid()
    {
        var (userId, jobApplicationId) = await SeedHelper.CreateUserWithJobApplicationAsync(_factory.Services);

        var command = new UpdateJobApplicationCommand
        {
            Id = jobApplicationId,
            UserId = userId,
            Name = "Valid Job",
            Position = "Valid",
            URL = "https://company.com/job",
            JobDescription = "Good job",
            Location = "Remote",
            ContactPerson = "Jane Doe",
            Status = JobApplicationStatus.New,
            StartDate = DateTime.UtcNow,
            DurationInMonths = "abc"
        };

        var response = await _client.PutAsJsonAsync($"/api/job-applications/{command.Id}", command);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        var problem = await response.Content.ReadFromJsonAsync<CustomProblemDetails>();
        problem.ShouldNotBeNull();
        problem.Errors.ShouldContainKey("DurationInMonths");
        problem.Errors["DurationInMonths"].Any(msg => msg.Contains("valid number")).ShouldBeTrue();
    }

    [Fact]
    public async Task UpdateJobApplication_ShouldReturn400_WhenJobApplicationDoesNotExist()
    {
        var (userId, _) = await SeedHelper.CreateUserWithJobApplicationAsync(_factory.Services);

        var command = new UpdateJobApplicationCommand
        {
            Id = 9999,
            UserId = userId,
            Name = "Valid Job",
            Position = "Valid",
            URL = "https://company.com/job",
            JobDescription = "Valid",
            Location = "Valid",
            ContactPerson = "Valid",
            Status = JobApplicationStatus.New,
            StartDate = DateTime.UtcNow,
            DurationInMonths = "12"
        };

        var response = await _client.PutAsJsonAsync($"/api/job-applications/{command.Id}", command);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        var problem = await response.Content.ReadFromJsonAsync<CustomProblemDetails>();
        problem.ShouldNotBeNull();
        problem.Errors.ShouldContainKey("");
        problem.Errors[""].Any(msg => msg.Contains("doesn't exist")).ShouldBeTrue();
    }

    [Fact]
    public async Task UpdateJobApplication_ShouldReturn400_WhenJobApplicationBelongsToAnotherUser()
    {
        var (userId, _) = await SeedHelper.CreateUserWithJobApplicationAsync(_factory.Services);
        var (_, jobApplicationId2) = await SeedHelper.CreateUserWithJobApplicationAsync(_factory.Services);

        var command = new UpdateJobApplicationCommand
        {
            Id = jobApplicationId2, // belonging to different user
            UserId = userId,
            Name = "Valid Job",
            Position = "Valid",
            URL = "https://company.com/job",
            JobDescription = "Valid",
            Location = "Valid",
            ContactPerson = "Valid",
            Status = JobApplicationStatus.New,
            StartDate = DateTime.UtcNow,
        };

        var response = await _client.PutAsJsonAsync($"/api/job-applications/{command.Id}", command);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        var problem = await response.Content.ReadFromJsonAsync<CustomProblemDetails>();
        problem.ShouldNotBeNull();
        problem.Errors.ShouldContainKey("");
        problem.Errors[""].Any(msg => msg.Contains("doesn't exist")).ShouldBeTrue();
    }
}
