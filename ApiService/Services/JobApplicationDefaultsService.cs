using AppTrack.Frontend.ApiService.Base;
using AppTrack.Frontend.ApiService.Contracts;
using AppTrack.Frontend.Models;
using AutoMapper;

namespace AppTrack.Frontend.ApiService.Services;

public class JobApplicationDefaultsService : BaseHttpService, IJobApplicationDefaultsService
{
    private readonly IMapper _mapper;

    public JobApplicationDefaultsService(IMapper mapper, IClient client, ITokenStorage tokenStorage) : base(client, tokenStorage)
    {
        this._mapper = mapper;
    }

    public Task<Response<JobApplicationDefaultsModel>> GetForUserAsync(string userId) =>
        TryExecuteAsync(async () =>
        {
            var jobApplicationDefaults = await _client.GetJobApplicationDefaultsForUserAsync(userId);
            return _mapper.Map<JobApplicationDefaultsModel>(jobApplicationDefaults);
        });


    public Task<Response<JobApplicationDefaultsModel>> UpdateAsync(int id, JobApplicationDefaultsModel jobApplicationDefaultsModel) =>
        TryExecuteAsync<JobApplicationDefaultsModel>(async () =>
        {
            var command = _mapper.Map<UpdateJobApplicationDefaultsCommand>(jobApplicationDefaultsModel);
            await _client.UpdateJobApplicationDefaultsAsync(id, command);
        });
}


