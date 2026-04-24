using AppTrack.Frontend.ApiService.Base;
using AppTrack.Frontend.ApiService.Contracts;
using AppTrack.Frontend.ApiService.Mappings;
using AppTrack.Frontend.Models;

namespace AppTrack.Frontend.ApiService.Services;

public class RssSettingsService : BaseHttpService, IRssSettingsService
{
    public RssSettingsService(IClient client) : base(client)
    {
    }

    public Task<Response<List<RssPortalModel>>> GetPortalsAsync() =>
        TryExecuteAsync(async () =>
        {
            var dtos = await _client.PortalsAsync();
            return dtos.Select(d => d.ToModel()).ToList();
        });

    public Task<Response<RssMonitoringSettingsModel>> GetSettingsAsync() =>
        TryExecuteAsync(async () =>
        {
            var dto = await _client.SettingsGETAsync();
            return dto.ToModel();
        });

    public Task<Response<bool>> SetSubscriptionsAsync(List<RssPortalModel> portals) =>
        TryExecuteAsync(async () =>
        {
            await _client.SubscriptionsAsync(portals.ToSetSubscriptionsCommand());
            return true;
        });

    public Task<Response<bool>> UpdateSettingsAsync(RssMonitoringSettingsModel model) =>
        TryExecuteAsync(async () =>
        {
            await _client.SettingsPUTAsync(model.ToUpdateSettingsCommand());
            return true;
        });
}
