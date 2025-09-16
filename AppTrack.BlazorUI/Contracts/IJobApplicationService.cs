using AppTrack.BlazorUI.Models.JobApplications;
using AppTrack.BlazorUI.Services.Base;

namespace AppTrack.BlazorUI.Contracts
{
    public interface IJobApplicationService
    {
        Task<List<JobApplicationVM>> GetJobApplicationsAsync();
        Task<JobApplicationVM> GetJobApplicationById(int id);
        Task<Response<Guid>> CreateJobApplication(JobApplicationVM jobApplication);
        Task<Response<Guid>> UpdateJobApplication(int id, JobApplicationVM jobApplication);
        Task<Response<Guid>> DeleteJobApplication(int id);
    }
}
