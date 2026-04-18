using AppTrack.Api.IntegrationTests.Seeddata;
using AppTrack.Application.Features.FreelancerProfile.Dto;
using Shouldly;
using System.Net;
using System.Net.Http.Json;

namespace AppTrack.Api.IntegrationTests.ProfileControllerTests;

public class GetProfileTests : IClassFixture<FakeAuthWebApplicationFactory>
{
    private readonly FakeAuthWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public GetProfileTests(FakeAuthWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateAuthenticatedClient();
    }

    [Fact]
    public async Task GetProfile_ShouldReturn200_WhenProfileExists()
    {
        await SeedHelper.CreateFreelancerProfileForTestUserAsync(_factory.Services);

        var response = await _client.GetAsync("/api/profile");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<FreelancerProfileDto>();
        result.ShouldNotBeNull();
        result.FirstName.ShouldBe("Test");
        result.LastName.ShouldBe("User");
    }
}
