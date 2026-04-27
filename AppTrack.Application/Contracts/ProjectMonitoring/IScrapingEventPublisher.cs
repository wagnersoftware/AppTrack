using AppTrack.Domain;

namespace AppTrack.Application.Contracts.ProjectMonitoring;

public interface IScrapingEventPublisher
{
    Task PublishScrapingCompletedAsync(IEnumerable<int> portalIds, CancellationToken ct);
}
