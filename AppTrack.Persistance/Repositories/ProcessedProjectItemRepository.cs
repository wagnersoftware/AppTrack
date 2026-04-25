using AppTrack.Application.Contracts.ProjectMonitoring;
using AppTrack.Domain;
using AppTrack.Persistance.DatabaseContext;
using Microsoft.EntityFrameworkCore;

namespace AppTrack.Persistance.Repositories;

public class ProcessedProjectItemRepository : GenericRepository<ProcessedProjectItem>, IProcessedProjectItemRepository
{
    public ProcessedProjectItemRepository(AppTrackDatabaseContext dbContext) : base(dbContext) { }

    public async Task<HashSet<string>> GetProcessedUrlsAsync(string userId, IEnumerable<string> urls)
    {
        var urlList = urls.ToList();
        var processed = await _context.ProcessedProjectItems.AsNoTracking()
            .Where(p => p.UserId == userId && urlList.Contains(p.ProjectItemUrl))
            .Select(p => p.ProjectItemUrl)
            .ToListAsync();
        return processed.ToHashSet();
    }

    public async Task AddRangeAsync(IEnumerable<ProcessedProjectItem> items, CancellationToken ct)
    {
        await _context.ProcessedProjectItems.AddRangeAsync(items, ct);
        await _context.SaveChangesAsync(ct);
    }
}
