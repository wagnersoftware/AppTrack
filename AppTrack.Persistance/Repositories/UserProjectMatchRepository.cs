using AppTrack.Application.Contracts.ProjectMonitoring;
using AppTrack.Domain;
using AppTrack.Persistance.DatabaseContext;
using Microsoft.EntityFrameworkCore;

namespace AppTrack.Persistance.Repositories;

public class UserProjectMatchRepository : GenericRepository<UserProjectMatch>, IUserProjectMatchRepository
{
    public UserProjectMatchRepository(AppTrackDatabaseContext dbContext) : base(dbContext) { }

    public async Task<UserProjectMatch?> GetByUserAndProjectAsync(string userId, int scrapedProjectId, CancellationToken ct)
        => await _context.UserProjectMatches.AsNoTracking()
            .FirstOrDefaultAsync(m => m.UserId == userId && m.ScrapedProjectId == scrapedProjectId, ct);

    public async Task<List<UserProjectMatch>> GetUnnotifiedMatchesAsync(string userId, CancellationToken ct)
        => await _context.UserProjectMatches.AsNoTracking()
            .Where(m => m.UserId == userId && !m.IsNotified)
            .Include(m => m.ScrapedProject)
            .ToListAsync(ct);

    public async Task MarkAsNotifiedAsync(string userId, int scrapedProjectId, CancellationToken ct)
    {
        var match = await _context.UserProjectMatches
            .FirstOrDefaultAsync(m => m.UserId == userId && m.ScrapedProjectId == scrapedProjectId, ct);

        if (match == null) return;

        match.IsNotified = true;
        _context.UserProjectMatches.Update(match);
        await _context.SaveChangesAsync(ct);
    }
}
