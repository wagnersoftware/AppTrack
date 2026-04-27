namespace AppTrack.Application.Contracts.ProjectMonitoring;

public interface IProjectDataCleanupRepository
{
    Task CleanupOlderThanAsync(DateTime cutoff, CancellationToken ct);
}
