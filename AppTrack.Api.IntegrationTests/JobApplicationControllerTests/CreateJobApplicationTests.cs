using AppTrack.Api.Models;
using AppTrack.Application.Features.JobApplications.Commands.CreateJobApplication;
using AppTrack.Application.Features.JobApplications.Dto;
using AppTrack.Domain.Enums;
using Shouldly;
using System.Net;
using System.Net.Http.Json;

namespace AppTrack.Api.IntegrationTests;

public class CreateJobApplicationTests : IClassFixture<FakeAuthWebApplicationFactory>
{
    private readonly HttpClient _client;

    public CreateJobApplicationTests(FakeAuthWebApplicationFactory factory)
    {
        _client = factory.CreateAuthenticatedClient();
    }

    [Fact]
    public async Task CreateJobApplication_ShouldReturn201_WhenCommandIsValid()
    {
        // Arrange
        var command = new CreateJobApplicationCommand
        {
            UserId = Guid.NewGuid().ToString(),
            Name = "Valid Job",
            Position = "Software Developer",
            URL = "https://company.com/job",
            JobDescription = "Great opportunity",
            Location = "Remote",
            ContactPerson = "Jane Doe",
            StartDate = DateTime.UtcNow,
            Status = JobApplicationStatus.New,
            DurationInMonths = "6"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/job-applications", command);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        var created = await response.Content.ReadFromJsonAsync<JobApplicationDto>();
        created.ShouldNotBeNull();
        created.Name.ShouldBe(command.Name);
    }

    [Theory]
    [InlineData("Name", 201)]
    [InlineData("Position", 201)]
    [InlineData("Location", 201)]
    [InlineData("ContactPerson", 201)]
    [InlineData("URL", 1001)]
    public async Task CreateJobApplication_ShouldReturn400_WhenTextFieldExceedsMaxLength(string propertyName, int maxLength)
    {
        // Arrange
        var longValue = new string('x', maxLength);
        var command = new CreateJobApplicationCommand
        {
            UserId = Guid.NewGuid().ToString(),
            Name = propertyName == "Name" ? longValue : "Valid",
            Position = propertyName == "Position" ? longValue : "Valid",
            URL = propertyName == "URL" ? longValue : "https://company.com/job",
            JobDescription = "Valid",
            Location = propertyName == "Location" ? longValue : "Valid",
            ContactPerson = propertyName == "ContactPerson" ? longValue : "Valid",
            StartDate = DateTime.UtcNow,
            Status = JobApplicationStatus.New
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/job-applications", command);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        var problem = await response.Content.ReadFromJsonAsync<CustomProblemDetails>();
        problem.ShouldNotBeNull();
        problem.Errors.ShouldContainKey(propertyName);
        problem.Errors[propertyName].Any(msg => msg.Contains("must not exceed")).ShouldBeTrue();
    }

    [Fact]
    public async Task CreateJobApplication_ShouldReturn400_WhenUrlIsInvalid()
    {
        var command = new CreateJobApplicationCommand
        {
            UserId = Guid.NewGuid().ToString(),
            Name = "Valid Job",
            Position = "Software Developer",
            URL = "invalid-url",
            JobDescription = "Good job",
            Location = "Remote",
            ContactPerson = "Jane Doe",
            StartDate = DateTime.UtcNow,
            Status = JobApplicationStatus.New
        };

        var response = await _client.PostAsJsonAsync("/api/job-applications", command);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        var problem = await response.Content.ReadFromJsonAsync<CustomProblemDetails>();
        problem.ShouldNotBeNull();
        problem.Errors.ShouldContainKey("URL");
        problem.Errors["URL"].Any(msg => msg.Contains("valid URL")).ShouldBeTrue();
    }

    [Fact]
    public async Task CreateJobApplication_ShouldReturn400_WhenRequiredFieldsMissing()
    {
        var command = new CreateJobApplicationCommand();

        var response = await _client.PostAsJsonAsync("/api/job-applications", command);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        var problem = await response.Content.ReadFromJsonAsync<CustomProblemDetails>();
        problem.ShouldNotBeNull();

        var requiredFields = new[]
        {
            "Name", "Position", "URL", "UserId", "JobDescription",
            "Location", "ContactPerson", "StartDate"
        };

        foreach (var field in requiredFields)
        {
            problem.Errors.ShouldContainKey(field);
            problem.Errors[field].Any(msg => msg.Contains("required")).ShouldBeTrue();
        }
    }

    [Fact]
    public async Task CreateJobApplication_ShouldReturn400_WhenDurationInMonthsIsNotNumber()
    {
        var command = new CreateJobApplicationCommand
        {
            UserId = Guid.NewGuid().ToString(),
            Name = "Valid Job",
            Position = "Software Developer",
            URL = "https://company.com/job",
            JobDescription = "Good job",
            Location = "Remote",
            ContactPerson = "Jane Doe",
            StartDate = DateTime.UtcNow,
            Status = JobApplicationStatus.New,
            DurationInMonths = "abc"
        };

        var response = await _client.PostAsJsonAsync("/api/job-applications", command);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        var problem = await response.Content.ReadFromJsonAsync<CustomProblemDetails>();
        problem.ShouldNotBeNull();
        problem.Errors.ShouldContainKey("DurationInMonths");
        problem.Errors["DurationInMonths"].Any(msg => msg.Contains("valid number")).ShouldBeTrue();
    }
}
