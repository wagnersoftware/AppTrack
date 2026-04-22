using AppTrack.Application.Contracts.RssFeed;
using AppTrack.Domain;
using AppTrack.Persistance.DatabaseContext;
using Microsoft.EntityFrameworkCore;

namespace AppTrack.Persistance.Repositories;

public class ProcessedFeedItemRepository : GenericRepository<ProcessedFeedItem>, IProcessedFeedItemRepository
{
    public ProcessedFeedItemRepository(AppTrackDatabaseContext dbContext) : base(dbContext) { }

    public async Task<HashSet<string>> GetProcessedUrlsAsync(string userId, IEnumerable<string> urls)
    {
        var urlList = urls.ToList();
        var processed = await _context.ProcessedFeedItems.AsNoTracking()
            .Where(p => p.UserId == userId && urlList.Contains(p.FeedItemUrl))
            .Select(p => p.FeedItemUrl)
            .ToListAsync();
        return processed.ToHashSet();
    }

    public async Task AddRangeAsync(IEnumerable<ProcessedFeedItem> items, CancellationToken ct)
    {
        await _context.ProcessedFeedItems.AddRangeAsync(items, ct);
        await _context.SaveChangesAsync(ct);
    }
}
