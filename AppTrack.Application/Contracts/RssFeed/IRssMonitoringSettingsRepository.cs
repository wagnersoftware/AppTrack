using AppTrack.Application.Contracts.Persistance;
using AppTrack.Domain;

namespace AppTrack.Application.Contracts.RssFeed;

public interface IRssMonitoringSettingsRepository : IGenericRepository<RssMonitoringSettings>
{
    Task<RssMonitoringSettings?> GetByUserIdAsync(string userId);
    Task UpsertAsync(RssMonitoringSettings settings);
}
