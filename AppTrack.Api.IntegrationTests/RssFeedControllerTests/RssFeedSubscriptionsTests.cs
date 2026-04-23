using AppTrack.Api.IntegrationTests.Seeddata;
using AppTrack.Application.Features.RssFeeds.Dto;
using Shouldly;
using System.Net;
using System.Net.Http.Json;

namespace AppTrack.Api.IntegrationTests.RssFeedControllerTests;

public class RssFeedSubscriptionsTests : IClassFixture<FakeAuthWebApplicationFactory>
{
    private readonly FakeAuthWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public RssFeedSubscriptionsTests(FakeAuthWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateAuthenticatedClient();
    }

    [Fact]
    public async Task SetSubscriptions_ShouldReturn204_WhenPortalIdIsValid()
    {
        var portalId = await SeedHelper.GetFirstRssPortalIdAsync(_factory.Services);
        var command = new { Subscriptions = new[] { new { PortalId = portalId, IsActive = true } } };

        var response = await _client.PutAsJsonAsync("api/rssfeeds/subscriptions", command);

        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task SetSubscriptions_ShouldReturn400_WhenPortalIdIsZero()
    {
        var command = new { Subscriptions = new[] { new { PortalId = 0, IsActive = true } } };

        var response = await _client.PutAsJsonAsync("api/rssfeeds/subscriptions", command);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task SetSubscriptions_ShouldPersistActivation_WhenCalledWithIsActiveTrue()
    {
        var portalId = await SeedHelper.GetFirstRssPortalIdAsync(_factory.Services);
        var command = new { Subscriptions = new[] { new { PortalId = portalId, IsActive = true } } };

        await _client.PutAsJsonAsync("api/rssfeeds/subscriptions", command);

        var response = await _client.GetAsync("api/rssfeeds/portals");
        var portals = await response.Content.ReadFromJsonAsync<List<RssPortalDto>>();
        portals.ShouldNotBeNull();
        portals.ShouldContain(p => p.Id == portalId && p.IsSubscribed);
    }

    [Fact]
    public async Task SetSubscriptions_ShouldReturn401_WhenUnauthenticated()
    {
        var unauthClient = _factory.CreateClient();
        var command = new { Subscriptions = new[] { new { PortalId = 1, IsActive = true } } };

        var response = await unauthClient.PutAsJsonAsync("api/rssfeeds/subscriptions", command);

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }
}
