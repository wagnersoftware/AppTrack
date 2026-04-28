using AppTrack.Application.Features.ProjectMonitoring.Commands.UpdateProjectMonitoringSettings;
using AppTrack.Application.Features.ProjectMonitoring.Dto;
using Shouldly;
using System.Net;
using System.Net.Http.Json;

namespace AppTrack.Api.IntegrationTests.ProjectMonitoringControllerTests;

public class ProjectMonitoringSettingsTests : IClassFixture<FakeAuthWebApplicationFactory>
{
    private readonly HttpClient _client;

    public ProjectMonitoringSettingsTests(FakeAuthWebApplicationFactory factory)
    {
        _client = factory.CreateAuthenticatedClient();
    }

    [Fact]
    public async Task GetSettings_ShouldReturn200_WithDefaultValues_WhenNoSettingsSaved()
    {
        // Act
        var response = await _client.GetAsync("api/projectmonitoring/settings");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var dto = await response.Content.ReadFromJsonAsync<ProjectMonitoringSettingsDto>();
        dto.ShouldNotBeNull();
        dto.NotificationIntervalMinutes.ShouldBe(60);
        dto.NotifyByEmail.ShouldBeFalse();
        dto.Keywords.ShouldBeEmpty();
    }

    [Fact]
    public async Task GetSettings_ShouldReturn200_WithSavedValues_WhenSettingsExist()
    {
        // Arrange — PUT settings first
        var command = new UpdateProjectMonitoringSettingsCommand
        {
            Keywords = ["dotnet", "remote"],
            NotifyByEmail = true,
            NotificationIntervalMinutes = 60
        };

        var putResponse = await _client.PutAsJsonAsync("api/projectmonitoring/settings", command);

        // Assert PUT
        putResponse.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        // Act — GET to verify round-trip
        var getResponse = await _client.GetAsync("api/projectmonitoring/settings");

        // Assert GET
        getResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        var dto = await getResponse.Content.ReadFromJsonAsync<ProjectMonitoringSettingsDto>();
        dto.ShouldNotBeNull();
        dto.Keywords.ShouldContain("dotnet");
        dto.Keywords.ShouldContain("remote");
        dto.NotifyByEmail.ShouldBeTrue();
    }

    [Fact]
    public async Task UpdateSettings_ShouldReturn204_WhenCommandIsValid()
    {
        // Arrange
        var command = new UpdateProjectMonitoringSettingsCommand
        {
            Keywords = ["csharp"],
            NotifyByEmail = false,
            NotificationIntervalMinutes = 120
        };

        // Act
        var response = await _client.PutAsJsonAsync("api/projectmonitoring/settings", command);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }
}
