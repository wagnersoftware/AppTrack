using AppTrack.Application.Contracts.Persistance;
using AppTrack.Domain;

namespace AppTrack.Application.Contracts.ProjectMonitoring;

public interface IProcessedProjectItemRepository : IGenericRepository<ProcessedProjectItem>
{
    Task<HashSet<string>> GetProcessedUrlsAsync(string userId, IEnumerable<string> urls);
    Task AddRangeAsync(IEnumerable<ProcessedProjectItem> items, CancellationToken ct);
}
