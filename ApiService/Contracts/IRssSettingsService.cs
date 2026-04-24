using AppTrack.Frontend.ApiService.Base;
using AppTrack.Frontend.Models;

namespace AppTrack.Frontend.ApiService.Contracts;

public interface IRssSettingsService
{
    Task<Response<List<RssPortalModel>>> GetPortalsAsync();
    Task<Response<RssMonitoringSettingsModel>> GetSettingsAsync();
    Task<Response<bool>> SetSubscriptionsAsync(List<RssPortalModel> portals);
    Task<Response<bool>> UpdateSettingsAsync(RssMonitoringSettingsModel model);
}
