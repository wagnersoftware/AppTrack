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

    public async Task<JobApplicationDefaultsModel> GetForUserAsync(int userId)
    {
        await AddBearerTokenAsync();
        var jobApplicationDefaults = await _client.GetJobApplicationDefaultsForUserAsync(userId.ToString());
        return _mapper.Map<JobApplicationDefaultsModel>(jobApplicationDefaults);
    }

    public async Task UpdateAsync(int id, JobApplicationDefaultsModel jobApplicationDefaultsModel)
    {
        await AddBearerTokenAsync();
        var command = _mapper.Map<UpdateJobApplicationDefaultsCommand>(jobApplicationDefaultsModel);
        await _client.UpdateJobApplicationDefaultsAsync(id, command);
    }
}
