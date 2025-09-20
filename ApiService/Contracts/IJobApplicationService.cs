using AppTrack.Frontend.ApiService.Base;
using AppTrack.Frontend.Models;

namespace AppTrack.Frontend.ApiService.Contracts;

public interface IJobApplicationService
{
    Task<List<JobApplicationModel>> GetJobApplicationsAsync();
    Task<JobApplicationModel> GetJobApplicationById(int id);
    Task<Response<Guid>> CreateJobApplication(JobApplicationModel jobApplication);
    Task<Response<Guid>> UpdateJobApplication(int id, JobApplicationModel jobApplication);
    Task<Response<Guid>> DeleteJobApplication(int id);
}
