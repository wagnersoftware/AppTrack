using AppTrack.Api.IntegrationTests.Seeddata;
using Shouldly;
using System.Net;

namespace AppTrack.Api.IntegrationTests.ProfileControllerTests;

public class DeleteProfileTests : IClassFixture<FakeAuthWebApplicationFactory>
{
    private readonly FakeAuthWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public DeleteProfileTests(FakeAuthWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateAuthenticatedClient();
    }

    [Fact]
    public async Task DeleteProfile_ShouldReturn204_WhenProfileExists()
    {
        await SeedHelper.CreateFreelancerProfileForTestUserAsync(_factory.Services);

        var response = await _client.DeleteAsync("/api/profile");

        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }
}
