using AppTrack.Frontend.ApiService.Base;
using AppTrack.Frontend.ApiService.Contracts;
using AppTrack.Frontend.ApiService.Mappings;
using AppTrack.Frontend.Models;

namespace AppTrack.Frontend.ApiService.Services;

public class ProjectMonitoringService : BaseHttpService, IProjectMonitoringService
{
    public ProjectMonitoringService(IClient client) : base(client)
    {
    }

    public Task<Response<List<ProjectPortalModel>>> GetPortalsAsync() =>
        TryExecuteAsync(async () =>
        {
            var dtos = await _client.PortalsAsync();
            return dtos.Select(d => d.ToModel()).ToList();
        });

    public Task<Response<ProjectMonitoringSettingsModel>> GetSettingsAsync() =>
        TryExecuteAsync(async () =>
        {
            var dto = await _client.SettingsGETAsync();
            return dto.ToModel();
        });

    public Task<Response<bool>> SetSubscriptionsAsync(List<ProjectPortalModel> portals) =>
        TryExecuteAsync(async () =>
        {
            await _client.SubscriptionsAsync(portals.ToSetSubscriptionsCommand());
            return true;
        });

    public Task<Response<bool>> UpdateSettingsAsync(ProjectMonitoringSettingsModel model) =>
        TryExecuteAsync(async () =>
        {
            await _client.SettingsPUTAsync(model.ToUpdateSettingsCommand());
            return true;
        });
}
