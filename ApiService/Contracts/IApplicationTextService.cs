using AppTrack.Frontend.ApiService.Base;

namespace AppTrack.Frontend.ApiService.Contracts;

public interface IApplicationTextService
{
    Task<Response<string>> GenerateApplicationText(int applicationId, int userId, string url, string position);
}
