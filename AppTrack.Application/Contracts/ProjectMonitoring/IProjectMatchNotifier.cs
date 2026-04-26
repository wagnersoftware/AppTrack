using AppTrack.Application.Features.ProjectMonitoring.Models;

namespace AppTrack.Application.Contracts.ProjectMonitoring;

public interface IProjectMatchNotifier
{
    Task NotifyAsync(string userId, string userEmail, List<ScrapedProjectData> matches, CancellationToken ct);
}
