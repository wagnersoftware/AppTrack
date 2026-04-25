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

    public async Task ReplaceForPortalAsync(int portalId, IEnumerable<ScrapedProject> projects, CancellationToken ct)
    {
        var existing = await _context.ScrapedProjects
            .Where(p => p.ProjectPortalId == portalId)
            .ToListAsync(ct);

        _context.ScrapedProjects.RemoveRange(existing);
        await _context.ScrapedProjects.AddRangeAsync(projects, ct);
        await _context.SaveChangesAsync(ct);
    }
}
