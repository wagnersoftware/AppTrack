using AppTrack.Frontend.ApiService.Base;
using AppTrack.Frontend.ApiService.Contracts;
using AppTrack.Frontend.Models;

namespace AppTrack.Frontend.ApiService.Services;

public class AiSettingsService : BaseHttpService, IAiSettingsService
{
    public AiSettingsService(IClient client, ITokenStorage tokenStorage) : base(client, tokenStorage)
    {
    }

    public Task<JobApplicationDefaultsModel> GetForUser(int userId)
    {
        throw new NotImplementedException();
    }
}
