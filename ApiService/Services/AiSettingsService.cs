using AppTrack.Frontend.ApiService.Base;
using AppTrack.Frontend.ApiService.Contracts;
using AppTrack.Frontend.ApiService.Mappings;
using AppTrack.Frontend.Models;

namespace AppTrack.Frontend.ApiService.Services;

public class AiSettingsService : BaseHttpService, IAiSettingsService
{
    public AiSettingsService(IClient client, ITokenStorage tokenStorage) : base(client, tokenStorage)
    {
    }

    public Task<Response<AiSettingsModel>> GetForUserAsync() =>
        TryExecuteAsync(async () =>
        {
            var aiSettingsDto = await _client.AiSettingsGETAsync();
            return aiSettingsDto.ToModel();
        });

    public Task<Response<AiSettingsModel>> UpdateAsync(int id, AiSettingsModel aiSettingsModel) =>
        TryExecuteAsync(async () =>
        {
            var dto = await _client.AiSettingsPUTAsync(id, aiSettingsModel.ToUpdateCommand());
            return dto.ToModel();
        });
}
