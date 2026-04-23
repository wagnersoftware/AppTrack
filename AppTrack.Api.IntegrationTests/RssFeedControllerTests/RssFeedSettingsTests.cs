using AppTrack.Api.IntegrationTests.Seeddata;
using AppTrack.Application.Features.RssFeeds.Dto;
using Shouldly;
using System.Net;
using System.Net.Http.Json;

namespace AppTrack.Api.IntegrationTests.RssFeedControllerTests;

public class RssFeedSettingsTests : IClassFixture<FakeAuthWebApplicationFactory>
{
    private readonly FakeAuthWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public RssFeedSettingsTests(FakeAuthWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateAuthenticatedClient();
    }

    [Fact]
    public async Task GetSettings_ShouldReturn200_WithDefaults_WhenNoSettingsExist()
    {
        var response = await _client.GetAsync("api/rssfeeds/settings");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var settings = await response.Content.ReadFromJsonAsync<RssMonitoringSettingsDto>();
        settings.ShouldNotBeNull();
        settings.Keywords.ShouldBeEmpty();
        settings.PollIntervalMinutes.ShouldBe(60);
    }

    [Fact]
    public async Task GetSettings_ShouldReturn200_WithPersistedSettings_WhenSettingsExist()
    {
        await SeedHelper.CreateRssMonitoringSettingsForTestUserAsync(
            _factory.Services, ["dotnet", "azure"], 30);

        var response = await _client.GetAsync("api/rssfeeds/settings");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var settings = await response.Content.ReadFromJsonAsync<RssMonitoringSettingsDto>();
        settings.ShouldNotBeNull();
        settings.Keywords.ShouldBe(["dotnet", "azure"]);
        settings.PollIntervalMinutes.ShouldBe(30);
    }

    [Fact]
    public async Task UpdateSettings_ShouldReturn204_WhenRequestIsValid()
    {
        var command = new { Keywords = new[] { "dotnet" }, PollIntervalMinutes = 60 };

        var response = await _client.PutAsJsonAsync("api/rssfeeds/settings", command);

        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task UpdateSettings_ShouldReturn400_WhenKeywordsIsNull()
    {
        var command = new { Keywords = (string[]?)null, PollIntervalMinutes = 60 };

        var response = await _client.PutAsJsonAsync("api/rssfeeds/settings", command);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateSettings_ShouldReturn400_WhenIntervalIsBelowMinimum()
    {
        var command = new { Keywords = new[] { "dotnet" }, PollIntervalMinutes = 0 };

        var response = await _client.PutAsJsonAsync("api/rssfeeds/settings", command);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateSettings_ShouldPersistSettings_WhenCalledWithValidData()
    {
        var command = new { Keywords = new[] { "blazor", "csharp" }, PollIntervalMinutes = 120 };

        await _client.PutAsJsonAsync("api/rssfeeds/settings", command);
        var response = await _client.GetAsync("api/rssfeeds/settings");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var settings = await response.Content.ReadFromJsonAsync<RssMonitoringSettingsDto>();
        settings.ShouldNotBeNull();
        settings.Keywords.ShouldBe(["blazor", "csharp"]);
        settings.PollIntervalMinutes.ShouldBe(120);
    }

    [Fact]
    public async Task GetSettings_ShouldReturn401_WhenUnauthenticated()
    {
        var unauthClient = _factory.CreateClient();
        var response = await unauthClient.GetAsync("api/rssfeeds/settings");
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UpdateSettings_ShouldReturn401_WhenUnauthenticated()
    {
        var unauthClient = _factory.CreateClient();
        var command = new { Keywords = new[] { "dotnet" }, PollIntervalMinutes = 60 };

        var response = await unauthClient.PutAsJsonAsync("api/rssfeeds/settings", command);

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }
}
