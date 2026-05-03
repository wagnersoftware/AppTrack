namespace AppTrack.Application.Contracts.ProjectMonitoring;

public interface IScrapingScheduleRepository
{
    Task<DateTime?> GetNextRunAfterAsync(CancellationToken ct);
    Task SetNextRunAfterAsync(DateTime nextRunAfterUtc, CancellationToken ct);
}
