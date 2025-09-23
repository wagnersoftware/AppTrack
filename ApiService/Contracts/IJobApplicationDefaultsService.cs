using AppTrack.Frontend.Models;

namespace AppTrack.Frontend.ApiService.Contracts;

public interface IJobApplicationDefaultsService
{
    Task<JobApplicationDefaultsModel> GetForUser(int userId);
}
