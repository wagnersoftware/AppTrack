using AppTrack.Application.Contracts.Persistance;
using AppTrack.Domain;

namespace AppTrack.Application.Contracts.RssFeed;

public interface IProcessedFeedItemRepository : IGenericRepository<ProcessedFeedItem>
{
    Task<HashSet<string>> GetProcessedUrlsAsync(string userId, IEnumerable<string> urls);
    Task AddRangeAsync(IEnumerable<ProcessedFeedItem> items, CancellationToken ct);
}
