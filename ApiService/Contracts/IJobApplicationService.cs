using AppTrack.Frontend.ApiService.Base;
using AppTrack.Frontend.Models;

namespace AppTrack.Frontend.ApiService.Contracts;

public interface IJobApplicationService
{
    Task<Response<List<JobApplicationModel>>> GetJobApplicationsAsync();
    Task<Response<JobApplicationModel>>GetJobApplicationByIdAsync(int id);
    Task<Response<JobApplicationModel>> CreateJobApplicationAsync(JobApplicationModel jobApplicationModel);
    Task<Response<JobApplicationModel>> UpdateJobApplicationAsync(int id, JobApplicationModel jobApplication);
    Task<Response<JobApplicationModel>> DeleteJobApplicationAsync(int id);
}
