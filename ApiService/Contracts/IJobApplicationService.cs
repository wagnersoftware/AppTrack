using AppTrack.Frontend.ApiService.Base;
using AppTrack.Frontend.Models;

namespace AppTrack.Frontend.ApiService.Contracts;

public interface IJobApplicationService
{
    Task<Response<List<JobApplicationModel>>> GetJobApplicationsForUserAsync();
    Task<Response<JobApplicationModel>> GetJobApplicationByIdAsync(int id);
    Task<Response<JobApplicationModel>> CreateJobApplicationForUserAsync(JobApplicationModel jobApplicationModel, string userId);
    Task<Response<JobApplicationModel>> UpdateJobApplicationAsync(int id, string userId, JobApplicationModel jobApplicationModel);
    Task<Response<JobApplicationModel>> DeleteJobApplicationAsync(int id);
}
