using AppTrack.Application.Features.JobApplications.Commands.CreateJobApplication;
using AppTrack.Application.Features.JobApplications.Dto;
using AppTrack.Domain.Enums;
using Shouldly;
using System.Net;
using System.Net.Http.Json;

namespace AppTrack.Api.IntegrationTests.JobApplicationControllerTests;

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
}
