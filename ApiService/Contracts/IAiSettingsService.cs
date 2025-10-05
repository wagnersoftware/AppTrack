using AppTrack.Frontend.ApiService.Base;
using AppTrack.Frontend.Models;

namespace AppTrack.Frontend.ApiService.Contracts;

public interface IAiSettingsService
{
    Task<Response<AiSettingsModel>> GetForUserAsync(string userId);

    Task<Response<AiSettingsModel>> UpdateAsync(int id, AiSettingsModel aiSettingsModel);
}
