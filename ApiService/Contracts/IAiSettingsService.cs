using AppTrack.Frontend.Models;

namespace AppTrack.Frontend.ApiService.Contracts;

public interface IAiSettingsService
{
    Task<JobApplicationDefaultsModel> GetForUser(int userId);
}
