using AppTrack.Application.Contracts.ProjectMonitoring;
using AppTrack.Application.Features.ProjectMonitoring.Models;

namespace AppTrack.Api.IntegrationTests.WebApplicationFactory;

public class StubProjectMatchNotifier : IProjectMatchNotifier
{
    public Task NotifyAsync(string userId, string userEmail, List<ScrapedProjectData> matches, CancellationToken ct)
        => Task.CompletedTask;
}
