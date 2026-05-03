using AppTrack.Application.Features.ProjectMonitoring.Commands.SetPortalSubscriptions;
using AppTrack.Application.Features.ProjectMonitoring.Dto;
using Shouldly;
using System.Net;
using System.Net.Http.Json;

namespace AppTrack.Api.IntegrationTests.ProjectMonitoringControllerTests;

public class ProjectMonitoringSubscriptionsTests : IClassFixture<FakeAuthWebApplicationFactory>
{
    private readonly HttpClient _client;

    public ProjectMonitoringSubscriptionsTests(FakeAuthWebApplicationFactory factory)
    {
        _client = factory.CreateAuthenticatedClient();
    }

    [Fact]
    public async Task SetSubscriptions_ShouldReturn204_WhenCommandIsValid()
    {
        // Arrange
        var command = new SetPortalSubscriptionsCommand
        {
            Subscriptions = [new PortalSubscriptionItemDto(1, true)]
        };

        // Act
        var response = await _client.PutAsJsonAsync("api/projectmonitoring/subscriptions", command);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task SetSubscriptions_ShouldPersistSubscription_WhenPortalIsActivated()
    {
        // Arrange
        var command = new SetPortalSubscriptionsCommand
        {
            Subscriptions = [new PortalSubscriptionItemDto(1, true)]
        };

        // Act — PUT to activate portal 1
        var putResponse = await _client.PutAsJsonAsync("api/projectmonitoring/subscriptions", command);
        putResponse.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        // Assert — GET portals and verify portal 1 is subscribed
        var getResponse = await _client.GetAsync("api/projectmonitoring/portals");
        getResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        var portals = await getResponse.Content.ReadFromJsonAsync<List<ProjectPortalDto>>();
        portals.ShouldNotBeNull();
        portals.ShouldContain(p => p.Id == 1 && p.IsSubscribed);
    }
}
