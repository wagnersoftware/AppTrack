using AppTrack.Application.Contracts.ProjectMonitoring;
using AppTrack.Domain;
using AppTrack.Persistance.DatabaseContext;
using Microsoft.EntityFrameworkCore;

namespace AppTrack.Persistance.Repositories;

public class UserProjectMatchRepository : IUserProjectMatchRepository
{
    private readonly AppTrackDatabaseContext _context;

    public UserProjectMatchRepository(AppTrackDatabaseContext context)
        => _context = context;

    public async Task AddRangeAsync(IEnumerable<UserProjectMatch> matches, CancellationToken ct)
    {
        await _context.UserProjectMatches.AddRangeAsync(matches, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<List<UserProjectMatch>> GetUnnotifiedAsync(CancellationToken ct)
    {
        var eligibleUserIds = await _context.ProjectMonitoringSettings
            .Where(s => s.NotifyByEmail && !string.IsNullOrEmpty(s.NotificationEmail))
            .Select(s => s.UserId)
            .ToListAsync(ct);

        return await _context.UserProjectMatches
            .Include(m => m.ScrapedProject)
                .ThenInclude(p => p.ProjectPortal)
            .Where(m => !m.IsNotified && eligibleUserIds.Contains(m.UserId))
            .ToListAsync(ct);
    }

    public async Task MarkNotifiedAsync(IEnumerable<int> matchIds, CancellationToken ct)
    {
        await _context.UserProjectMatches
            .Where(m => matchIds.Contains(m.Id))
            .ExecuteUpdateAsync(s => s.SetProperty(m => m.IsNotified, true), ct);
    }
}
