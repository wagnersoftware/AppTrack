using AppTrack.Api.IntegrationTests.Seeddata;
using AppTrack.Api.IntegrationTests.WebApplicationFactory;
using Shouldly;
using System.Net;

namespace AppTrack.Api.IntegrationTests.ProfileControllerTests;

/// <summary>
/// DeleteFreelancerProfileCommandHandler depends on ICvStorageService (deletes CV blob if present).
/// FakeCvStorageWebApplicationFactory is required so DI resolves ICvStorageService
/// without needing real Azure configuration.
/// </summary>
public class DeleteProfileTests : IClassFixture<FakeCvStorageWebApplicationFactory>
{
    private readonly FakeCvStorageWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public DeleteProfileTests(FakeCvStorageWebApplicationFactory factory)
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
