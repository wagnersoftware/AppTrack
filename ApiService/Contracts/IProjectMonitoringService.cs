using AppTrack.Frontend.ApiService.Base;

namespace AppTrack.Frontend.ApiService.Contracts;

public interface IProjectMonitoringService
{
    Task<Response<ICollection<ProjectPortalDto>>> GetPortalsAsync();
    Task<Response<ProjectMonitoringSettingsDto>> GetSettingsAsync();
    Task<Response<bool>> SetSubscriptionsAsync(SetPortalSubscriptionsCommand command);
    Task<Response<bool>> UpdateSettingsAsync(UpdateProjectMonitoringSettingsCommand command);
}
