using AppTrack.Frontend.ApiService.Base;
using AppTrack.Frontend.ApiService.Contracts;
using AppTrack.Frontend.Models;
using AutoMapper;

namespace AppTrack.Frontend.ApiService.Services;

public class JobApplicationService : BaseHttpService, IJobApplicationService
{
    private readonly IMapper _mapper;

    public JobApplicationService(IClient client, IMapper mapper, ITokenStorage tokenStorage) : base(client, tokenStorage)
    {
        this._mapper = mapper;
    }

    public Task<Response<JobApplicationModel>> CreateJobApplicationForUserAsync(JobApplicationModel jobApplicationModel, string userId) =>
        TryExecuteAsync(async () =>
        {
            var createJobApplicationCommand = _mapper.Map<CreateJobApplicationCommand>(jobApplicationModel);
            createJobApplicationCommand.UserId = userId;
            var jobApplicationDto = await _client.JobApplicationsPOSTAsync(createJobApplicationCommand);
            return _mapper.Map<JobApplicationModel>(jobApplicationDto);
        });

    public Task<Response<JobApplicationModel>> DeleteJobApplicationAsync(int id, string userId) =>
        TryExecuteAsync<JobApplicationModel>(async () =>
        {
            await _client.JobApplicationsDELETEAsync(id, userId);
        });

    public Task<Response<JobApplicationModel>> GetJobApplicationByIdAsync(int id) =>
        TryExecuteAsync(async () =>
        {
            var jobApplicationDtos = await _client.JobApplicationsGETAsync(id);
            return _mapper.Map<JobApplicationModel>(jobApplicationDtos);
        });

    public Task<Response<List<JobApplicationModel>>> GetJobApplicationsForUserAsync(string userId) =>
        TryExecuteAsync(async () =>
        {
            var jobApplicationDtos = await _client.JobApplicationsAllAsync(userId);
            return _mapper.Map<List<JobApplicationModel>>(jobApplicationDtos);
        });


    public Task<Response<JobApplicationModel>> UpdateJobApplicationAsync(int id, string userId, JobApplicationModel jobApplicationModel) =>
        TryExecuteAsync(async () =>
        {
            var jobApplicationCommand = _mapper.Map<UpdateJobApplicationCommand>(jobApplicationModel);
            jobApplicationCommand.UserId = userId;
            var jobApplicationDto = await _client.JobApplicationsPUTAsync(id, jobApplicationCommand);
            return _mapper.Map<JobApplicationModel>(jobApplicationDto);
        });
}
