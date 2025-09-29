using AppTrack.Frontend.ApiService.Base;
using AppTrack.Frontend.Models;

namespace AppTrack.Frontend.ApiService.Contracts;

public interface IJobApplicationDefaultsService
{
    Task<Response<JobApplicationDefaultsModel>> GetForUserAsync(int userId);

    Task<Response<JobApplicationDefaultsModel>> UpdateAsync(int id, JobApplicationDefaultsModel jobApplicationDefaultsModel);
}
