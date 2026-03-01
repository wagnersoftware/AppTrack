using AppTrack.Frontend.ApiService.Base;
using AppTrack.Frontend.ApiService.Contracts;
using AppTrack.Frontend.ApiService.Mappings;
using AppTrack.Frontend.Models;

namespace AppTrack.Frontend.ApiService.Services;

public class JobApplicationService : BaseHttpService, IJobApplicationService
{
    public JobApplicationService(IClient client, ITokenStorage tokenStorage) : base(client, tokenStorage)
    {
    }

    public Task<Response<JobApplicationModel>> CreateJobApplicationForUserAsync(JobApplicationModel jobApplicationModel, string userId) =>
        TryExecuteAsync(async () =>
        {
            var createJobApplicationCommand = jobApplicationModel.ToCreateCommand();
            createJobApplicationCommand.UserId = userId;
            var jobApplicationDto = await _client.JobApplicationsPOSTAsync(createJobApplicationCommand);
            return jobApplicationDto.ToModel();
        });

    public Task<Response<JobApplicationModel>> DeleteJobApplicationAsync(int id, string userId) =>
        TryExecuteAsync<JobApplicationModel>(async () =>
        {
            await _client.JobApplicationsDELETEAsync(id, userId);
        });

    public Task<Response<JobApplicationModel>> GetJobApplicationByIdAsync(int id, string userId) =>
        TryExecuteAsync(async () =>
        {
            var jobApplicationDto = await _client.JobApplicationsGETAsync(id, userId);
            return jobApplicationDto.ToModel();
        });

    public Task<Response<List<JobApplicationModel>>> GetJobApplicationsForUserAsync(string userId) =>
        TryExecuteAsync(async () =>
        {
            var jobApplicationDtos = await _client.JobApplicationsAllAsync(userId);
            return jobApplicationDtos.Select(dto => dto.ToModel()).ToList();
        });

    public Task<Response<JobApplicationModel>> UpdateJobApplicationAsync(int id, string userId, JobApplicationModel jobApplicationModel) =>
        TryExecuteAsync(async () =>
        {
            var jobApplicationCommand = jobApplicationModel.ToUpdateCommand();
            jobApplicationCommand.UserId = userId;
            var jobApplicationDto = await _client.JobApplicationsPUTAsync(id, jobApplicationCommand);
            return jobApplicationDto.ToModel();
        });
}
