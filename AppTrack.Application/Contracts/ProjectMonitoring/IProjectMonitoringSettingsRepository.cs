using AppTrack.Application.Contracts.Persistance;
using AppTrack.Domain;

namespace AppTrack.Application.Contracts.ProjectMonitoring;

public interface IProjectMonitoringSettingsRepository : IGenericRepository<ProjectMonitoringSettings>
{
    Task<ProjectMonitoringSettings?> GetByUserIdAsync(string userId);
    Task UpsertAsync(ProjectMonitoringSettings settings);
}
