using AppTrack.Api.IntegrationTests.Auth;
using AppTrack.Api.IntegrationTests.Seeddata;
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
        var jobApplicationId = await SeedHelper.CreateJobApplicationForTestUserAsync(_factory.Services);

        var command = new UpdateJobApplicationCommand
        {
            Id = jobApplicationId,
            UserId = TestAuthHandler.TestUserId,
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
}
