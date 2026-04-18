using AppTrack.Application.Features.FreelancerProfile.Commands.UpsertFreelancerProfile;
using AppTrack.Application.Features.FreelancerProfile.Dto;
using Shouldly;
using System.Net;
using System.Net.Http.Json;

namespace AppTrack.Api.IntegrationTests.ProfileControllerTests;

public class UpsertProfileTests : IClassFixture<FakeAuthWebApplicationFactory>
{
    private readonly HttpClient _client;

    public UpsertProfileTests(FakeAuthWebApplicationFactory factory)
    {
        _client = factory.CreateAuthenticatedClient();
    }

    [Fact]
    public async Task UpsertProfile_ShouldReturn200_WhenRequestIsValid()
    {
        var request = new UpsertFreelancerProfileCommand
        {
            FirstName = "Jane",
            LastName = "Doe",
            HourlyRate = 120m,
            Skills = "C#, Blazor",
        };

        var response = await _client.PutAsJsonAsync("/api/profile", request);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<FreelancerProfileDto>();
        result.ShouldNotBeNull();
        result.FirstName.ShouldBe("Jane");
        result.LastName.ShouldBe("Doe");
        result.HourlyRate.ShouldBe(120m);
    }

    [Fact]
    public async Task UpsertProfile_ShouldReturn400_WhenHourlyRateIsNegative()
    {
        var request = new UpsertFreelancerProfileCommand
        {
            FirstName = "Jane",
            HourlyRate = -50m,
        };

        var response = await _client.PutAsJsonAsync("/api/profile", request);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }
}
