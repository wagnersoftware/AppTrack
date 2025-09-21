using AppTrack.Frontend.ApiService.Base;
using AppTrack.Frontend.Models;

namespace AppTrack.Frontend.ApiService.Contracts;

public interface IJobApplicationService
{
    Task<List<JobApplicationModel>> GetJobApplicationsAsync();
    Task<JobApplicationModel> GetJobApplicationById(int id);
    Task<Response<JobApplicationModel>> CreateJobApplication(JobApplicationModel jobApplicationModel);
    Task<Response<JobApplicationModel>> UpdateJobApplication(int id, JobApplicationModel jobApplication);
    Task<Response<JobApplicationModel>> DeleteJobApplication(int id);
}
