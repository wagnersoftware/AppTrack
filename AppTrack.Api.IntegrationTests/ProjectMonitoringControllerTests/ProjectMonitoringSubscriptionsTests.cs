using AppTrack.Api.IntegrationTests.Seeddata;
using AppTrack.Application.Features.ProjectMonitoring.Dto;
using Shouldly;
using System.Net;
using System.Net.Http.Json;

namespace AppTrack.Api.IntegrationTests.ProjectMonitoringControllerTests;

public class ProjectMonitoringSubscriptionsTests : IClassFixture<FakeAuthWebApplicationFactory>
{
    private readonly FakeAuthWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public ProjectMonitoringSubscriptionsTests(FakeAuthWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateAuthenticatedClient();
    }

    [Fact]
    public async Task SetSubscriptions_ShouldReturn204_WhenPortalIdIsValid()
    {
        var portalId = await SeedHelper.GetFirstProjectPortalIdAsync(_factory.Services);
        var command = new { Subscriptions = new[] { new { PortalId = portalId, IsActive = true } } };

        var response = await _client.PutAsJsonAsync("api/projectmonitoring/subscriptions", command);

        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task SetSubscriptions_ShouldReturn400_WhenPortalIdIsZero()
    {
        var command = new { Subscriptions = new[] { new { PortalId = 0, IsActive = true } } };

        var response = await _client.PutAsJsonAsync("api/projectmonitoring/subscriptions", command);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task SetSubscriptions_ShouldPersistActivation_WhenCalledWithIsActiveTrue()
    {
        var portalId = await SeedHelper.GetFirstProjectPortalIdAsync(_factory.Services);
        var command = new { Subscriptions = new[] { new { PortalId = portalId, IsActive = true } } };

        await _client.PutAsJsonAsync("api/projectmonitoring/subscriptions", command);

        var response = await _client.GetAsync("api/projectmonitoring/portals");
        var portals = await response.Content.ReadFromJsonAsync<List<ProjectPortalDto>>();
        portals.ShouldNotBeNull();
        portals.ShouldContain(p => p.Id == portalId && p.IsSubscribed);
    }

    [Fact]
    public async Task SetSubscriptions_ShouldReturn401_WhenUnauthenticated()
    {
        var unauthClient = _factory.CreateClient();
        var command = new { Subscriptions = new[] { new { PortalId = 1, IsActive = true } } };

        var response = await unauthClient.PutAsJsonAsync("api/projectmonitoring/subscriptions", command);

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }
}
