using AppTrack.Frontend.ApiService.Base;
using AppTrack.Frontend.Models;

namespace AppTrack.Frontend.ApiService.Contracts;

public interface IProjectMonitoringService
{
    Task<Response<List<ProjectPortalModel>>> GetPortalsAsync();
    Task<Response<ProjectMonitoringSettingsModel>> GetSettingsAsync();
    Task<Response<bool>> SetSubscriptionsAsync(List<ProjectPortalModel> portals);
    Task<Response<bool>> UpdateSettingsAsync(ProjectMonitoringSettingsModel model);
}
