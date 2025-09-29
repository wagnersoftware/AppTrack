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

    public Task<Response<JobApplicationModel>> CreateJobApplicationAsync(JobApplicationModel model) =>
        TryExecuteAsync(async () =>
        {
            var createJobApplicationCommand = _mapper.Map<CreateJobApplicationCommand>(model);
            var jobApplicationDto = await _client.JobApplicationsPOSTAsync(createJobApplicationCommand);
            return _mapper.Map<JobApplicationModel>(jobApplicationDto);
        });

    public Task<Response<JobApplicationModel>> DeleteJobApplicationAsync(int id) =>
        TryExecuteAsync<JobApplicationModel>(async () =>
        {
            await _client.JobApplicationsDELETEAsync(id);
        });

    public Task<Response<JobApplicationModel>> GetJobApplicationByIdAsync(int id) =>
        TryExecuteAsync(async () =>
        {
            var jobApplicationDtos = await _client.JobApplicationsGETAsync(id);
            return _mapper.Map<JobApplicationModel>(jobApplicationDtos);
        });

    public Task<Response<List<JobApplicationModel>>> GetJobApplicationsAsync() =>
        TryExecuteAsync(async () =>
        {
            var jobApplicationDtos = await _client.JobApplicationsAllAsync();
            return _mapper.Map<List<JobApplicationModel>>(jobApplicationDtos);
        });


    public Task<Response<JobApplicationModel>> UpdateJobApplicationAsync(int id, JobApplicationModel model) =>
        TryExecuteAsync<JobApplicationModel>(async () =>
        {
            var jobApplicationCommand = _mapper.Map<UpdateJobApplicationCommand>(model);
            await _client.JobApplicationsPUTAsync(id.ToString(), jobApplicationCommand);
        });
}
