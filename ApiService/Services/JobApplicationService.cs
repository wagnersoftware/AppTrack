using AppTrack.Frontend.ApiService.Base;
using AppTrack.Frontend.ApiService.Contracts;
using AppTrack.Frontend.ApiService.Mappings;
using AppTrack.Frontend.Models;

namespace AppTrack.Frontend.ApiService.Services;

public class JobApplicationService : BaseHttpService, IJobApplicationService
{
    public JobApplicationService(IClient client) : base(client)
    {
    }

    public Task<Response<JobApplicationModel>> CreateJobApplicationForUserAsync(JobApplicationModel jobApplicationModel) =>
        TryExecuteAsync(async () =>
        {
            var createJobApplicationCommand = jobApplicationModel.ToCreateCommand();
            var jobApplicationDto = await _client.JobApplicationsPOSTAsync(createJobApplicationCommand);
            return jobApplicationDto.ToModel();
        });

    public Task<Response<JobApplicationModel>> DeleteJobApplicationAsync(int id) =>
        TryExecuteAsync<JobApplicationModel>(async () =>
        {
            await _client.JobApplicationsDELETEAsync(id);
        });

    public Task<Response<JobApplicationModel>> GetJobApplicationByIdAsync(int id) =>
        TryExecuteAsync(async () =>
        {
            var jobApplicationDto = await _client.JobApplicationsGETAsync(id);
            return jobApplicationDto.ToModel();
        });

    public Task<Response<List<JobApplicationModel>>> GetJobApplicationsForUserAsync() =>
        TryExecuteAsync(async () =>
        {
            var jobApplicationDtos = await _client.JobApplicationsAllAsync();
            return jobApplicationDtos.Select(dto => dto.ToModel()).ToList();
        });

    public Task<Response<JobApplicationModel>> UpdateJobApplicationAsync(int id, JobApplicationModel jobApplicationModel) =>
        TryExecuteAsync(async () =>
        {
            var jobApplicationCommand = jobApplicationModel.ToUpdateCommand();
            var jobApplicationDto = await _client.JobApplicationsPUTAsync(id, jobApplicationCommand);
            return jobApplicationDto.ToModel();
        });
}
