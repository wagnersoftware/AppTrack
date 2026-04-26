using AppTrack.Application.Contracts.ProjectMonitoring;
using AppTrack.Domain;
using AppTrack.Persistance.DatabaseContext;
using Microsoft.EntityFrameworkCore;

namespace AppTrack.Persistance.Repositories;

public class ProjectMonitoringSettingsRepository : GenericRepository<ProjectMonitoringSettings>, IProjectMonitoringSettingsRepository
{
    public ProjectMonitoringSettingsRepository(AppTrackDatabaseContext dbContext) : base(dbContext) { }

    public async Task<ProjectMonitoringSettings?> GetByUserIdAsync(string userId)
        => await _context.ProjectMonitoringSettings.AsNoTracking()
            .FirstOrDefaultAsync(s => s.UserId == userId);

    public async Task UpsertAsync(ProjectMonitoringSettings settings)
    {
        var existing = await _context.ProjectMonitoringSettings
            .FirstOrDefaultAsync(s => s.UserId == settings.UserId);
        if (existing is null)
        {
            await _context.ProjectMonitoringSettings.AddAsync(settings);
        }
        else
        {
            existing.Keywords = settings.Keywords;
            existing.NotificationIntervalMinutes = settings.NotificationIntervalMinutes;
            existing.PollIntervalMinutes = settings.PollIntervalMinutes;
            existing.NotificationEmail = settings.NotificationEmail;
            existing.NotifyByEmail = settings.NotifyByEmail;
        }
        await _context.SaveChangesAsync();
    }
}
