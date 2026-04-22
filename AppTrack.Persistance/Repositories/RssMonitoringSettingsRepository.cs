using AppTrack.Application.Contracts.RssFeed;
using AppTrack.Domain;
using AppTrack.Persistance.DatabaseContext;
using Microsoft.EntityFrameworkCore;

namespace AppTrack.Persistance.Repositories;

public class RssMonitoringSettingsRepository : GenericRepository<RssMonitoringSettings>, IRssMonitoringSettingsRepository
{
    public RssMonitoringSettingsRepository(AppTrackDatabaseContext dbContext) : base(dbContext) { }

    public async Task<RssMonitoringSettings?> GetByUserIdAsync(string userId)
        => await _context.RssMonitoringSettings.AsNoTracking()
            .FirstOrDefaultAsync(s => s.UserId == userId);

    public async Task UpsertAsync(RssMonitoringSettings settings)
    {
        var existing = await _context.RssMonitoringSettings
            .FirstOrDefaultAsync(s => s.UserId == settings.UserId);
        if (existing is null)
        {
            await _context.RssMonitoringSettings.AddAsync(settings);
        }
        else
        {
            existing.Keywords = settings.Keywords;
            existing.PollIntervalMinutes = settings.PollIntervalMinutes;
            existing.NotificationEmail = settings.NotificationEmail;
        }
        await _context.SaveChangesAsync();
    }
}
