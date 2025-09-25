using AppTrack.Frontend.Models;

namespace AppTrack.Frontend.ApiService.Contracts;

public interface IAiSettingsService
{
    Task<AiSettingsModel> GetForUserAsync(int userId);

    Task UpdateAsync(int id, AiSettingsModel aiSettingsModel);
}
