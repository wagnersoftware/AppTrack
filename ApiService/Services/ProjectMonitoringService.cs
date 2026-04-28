using AppTrack.Frontend.ApiService.Base;
using AppTrack.Frontend.ApiService.Contracts;

namespace AppTrack.Frontend.ApiService.Services;

public class ProjectMonitoringService : BaseHttpService, IProjectMonitoringService
{
    public ProjectMonitoringService(IClient client) : base(client)
    {
    }

    public Task<Response<ICollection<ProjectPortalDto>>> GetPortalsAsync() =>
        TryExecuteAsync(() => _client.PortalsAsync());

    public Task<Response<ProjectMonitoringSettingsDto>> GetSettingsAsync() =>
        TryExecuteAsync(() => _client.SettingsGETAsync());

    public Task<Response<bool>> SetSubscriptionsAsync(SetPortalSubscriptionsCommand command) =>
        TryExecuteAsync<bool>(async () =>
        {
            await _client.SubscriptionsAsync(command);
            return true;
        });

    public Task<Response<bool>> UpdateSettingsAsync(UpdateProjectMonitoringSettingsCommand command) =>
        TryExecuteAsync<bool>(async () =>
        {
            await _client.SettingsPUTAsync(command);
            return true;
        });
}
