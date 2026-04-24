using AppTrack.Api.IntegrationTests.Seeddata;
using AppTrack.Application.Features.RssFeeds.Dto;
using Shouldly;
using System.Net;
using System.Net.Http.Json;

namespace AppTrack.Api.IntegrationTests.RssFeedControllerTests;

public class RssFeedPortalsTests : IClassFixture<FakeAuthWebApplicationFactory>
{
    private readonly FakeAuthWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public RssFeedPortalsTests(FakeAuthWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateAuthenticatedClient();
    }

    [Fact]
    public async Task GetPortals_ShouldReturn200_WithAllSystemPortals()
    {
        var response = await _client.GetAsync("api/rssfeeds/portals");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var portals = await response.Content.ReadFromJsonAsync<List<RssPortalDto>>();
        portals.ShouldNotBeNull();
        portals.ShouldNotBeEmpty();
        portals.ShouldContain(p => p.Name == "Freelancermap");
    }

    [Fact]
    public async Task GetPortals_ShouldReturn200_WithIsSubscribedFalse_WhenNoSubscription()
    {
        var response = await _client.GetAsync("api/rssfeeds/portals");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var portals = await response.Content.ReadFromJsonAsync<List<RssPortalDto>>();
        portals.ShouldNotBeNull();
        portals.ShouldAllBe(p => !p.IsSubscribed);
    }

    [Fact]
    public async Task GetPortals_ShouldReturn200_WithIsSubscribedTrue_WhenSubscriptionExists()
    {
        var portalId = await SeedHelper.GetFirstRssPortalIdAsync(_factory.Services);
        await SeedHelper.CreateRssSubscriptionForTestUserAsync(_factory.Services, portalId, isActive: true);

        var response = await _client.GetAsync("api/rssfeeds/portals");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var portals = await response.Content.ReadFromJsonAsync<List<RssPortalDto>>();
        portals.ShouldNotBeNull();
        portals.ShouldContain(p => p.Id == portalId && p.IsSubscribed);
    }

    [Fact]
    public async Task GetPortals_ShouldReturn401_WhenUnauthenticated()
    {
        var unauthClient = _factory.CreateClient();
        var response = await unauthClient.GetAsync("api/rssfeeds/portals");
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }
}
