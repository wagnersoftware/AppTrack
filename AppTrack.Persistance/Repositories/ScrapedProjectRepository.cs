using AppTrack.Application.Contracts.ProjectMonitoring;
using AppTrack.Domain;
using AppTrack.Persistance.DatabaseContext;
using Microsoft.EntityFrameworkCore;

namespace AppTrack.Persistance.Repositories;

public class ScrapedProjectRepository : GenericRepository<ScrapedProject>, IScrapedProjectRepository
{
    public ScrapedProjectRepository(AppTrackDatabaseContext dbContext) : base(dbContext) { }

    public async Task<List<ScrapedProject>> GetByPortalIdsAsync(IEnumerable<int> portalIds)
        => await _context.ScrapedProjects
            .AsNoTracking()
            .Include(p => p.ProjectPortal)
            .Where(p => portalIds.Contains(p.ProjectPortalId))
            .ToListAsync();

    public async Task AddNewForPortalAsync(int portalId, IEnumerable<ScrapedProject> projects, CancellationToken ct)
    {
        var existingUrls = (await _context.ScrapedProjects
            .Where(p => p.ProjectPortalId == portalId)
            .Select(p => p.Url)
            .ToListAsync(ct))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var newProjects = projects.Where(p => !existingUrls.Contains(p.Url)).ToList();

        if (newProjects.Count == 0) return;

        await _context.ScrapedProjects.AddRangeAsync(newProjects, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<List<ScrapedProject>> GetUnprocessedForUserAsync(
        string userId,
        IEnumerable<int> portalIds,
        CancellationToken ct)
    {
        var processedUrls = await _context.ProcessedProjectItems
            .Where(p => p.UserId == userId)
            .Select(p => p.ProjectItemUrl)
            .ToHashSetAsync(StringComparer.OrdinalIgnoreCase, ct);

        return await _context.ScrapedProjects
            .AsNoTracking()
            .Include(p => p.ProjectPortal)
            .Where(p => portalIds.Contains(p.ProjectPortalId)
                     && !processedUrls.Contains(p.Url))
            .ToListAsync(ct);
    }
}
