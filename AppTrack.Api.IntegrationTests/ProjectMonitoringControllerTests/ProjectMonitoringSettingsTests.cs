using AppTrack.Api.IntegrationTests.Seeddata;
using AppTrack.Application.Features.ProjectMonitoring.Dto;
using Shouldly;
using System.Net;
using System.Net.Http.Json;

namespace AppTrack.Api.IntegrationTests.ProjectMonitoringControllerTests;

public class ProjectMonitoringSettingsTests : IClassFixture<FakeAuthWebApplicationFactory>
{
    private readonly FakeAuthWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public ProjectMonitoringSettingsTests(FakeAuthWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateAuthenticatedClient();
    }

    [Fact]
    public async Task GetSettings_ShouldReturn200_WithDefaults_WhenNoSettingsExist()
    {
        var response = await _client.GetAsync("api/projectmonitoring/settings");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var settings = await response.Content.ReadFromJsonAsync<ProjectMonitoringSettingsDto>();
        settings.ShouldNotBeNull();
        settings.Keywords.ShouldBeEmpty();
        settings.NotificationIntervalMinutes.ShouldBe(60);
    }

    [Fact]
    public async Task GetSettings_ShouldReturn200_WithPersistedSettings_WhenSettingsExist()
    {
        await SeedHelper.CreateProjectMonitoringSettingsForTestUserAsync(
            _factory.Services, ["dotnet", "azure"], 30);

        var response = await _client.GetAsync("api/projectmonitoring/settings");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var settings = await response.Content.ReadFromJsonAsync<ProjectMonitoringSettingsDto>();
        settings.ShouldNotBeNull();
        settings.Keywords.ShouldBe(["dotnet", "azure"]);
        settings.NotificationIntervalMinutes.ShouldBe(30);
    }

    [Fact]
    public async Task UpdateSettings_ShouldReturn204_WhenRequestIsValid()
    {
        var command = new { Keywords = new[] { "dotnet" }, NotificationIntervalMinutes = 60 };

        var response = await _client.PutAsJsonAsync("api/projectmonitoring/settings", command);

        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task UpdateSettings_ShouldReturn400_WhenKeywordsIsNull()
    {
        var command = new { Keywords = (string[]?)null, NotificationIntervalMinutes = 60 };

        var response = await _client.PutAsJsonAsync("api/projectmonitoring/settings", command);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateSettings_ShouldReturn400_WhenIntervalIsBelowMinimum()
    {
        var command = new { Keywords = new[] { "dotnet" }, NotificationIntervalMinutes = 0 };

        var response = await _client.PutAsJsonAsync("api/projectmonitoring/settings", command);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateSettings_ShouldPersistSettings_WhenCalledWithValidData()
    {
        var command = new { Keywords = new[] { "blazor", "csharp" }, NotificationIntervalMinutes = 120 };

        await _client.PutAsJsonAsync("api/projectmonitoring/settings", command);
        var response = await _client.GetAsync("api/projectmonitoring/settings");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var settings = await response.Content.ReadFromJsonAsync<ProjectMonitoringSettingsDto>();
        settings.ShouldNotBeNull();
        settings.Keywords.ShouldBe(["blazor", "csharp"]);
        settings.NotificationIntervalMinutes.ShouldBe(120);
    }

    [Fact]
    public async Task GetSettings_ShouldReturn401_WhenUnauthenticated()
    {
        var unauthClient = _factory.CreateClient();
        var response = await unauthClient.GetAsync("api/projectmonitoring/settings");
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UpdateSettings_ShouldReturn401_WhenUnauthenticated()
    {
        var unauthClient = _factory.CreateClient();
        var command = new { Keywords = new[] { "dotnet" }, NotificationIntervalMinutes = 60 };

        var response = await unauthClient.PutAsJsonAsync("api/projectmonitoring/settings", command);

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }
}
