using AppTrack.Frontend.ApiService.Base;
using AppTrack.Frontend.Models;

namespace AppTrack.Frontend.ApiService.Contracts;

public interface IJobApplicationDefaultsService
{
    Task<JobApplicationDefaultsModel> GetForUserAsync(int userId);

    Task UpdateAsync(int id, JobApplicationDefaultsModel jobApplicationDefaultsModel);
}
