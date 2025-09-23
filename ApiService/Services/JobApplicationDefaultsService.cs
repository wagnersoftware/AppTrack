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

    public async Task<JobApplicationDefaultsModel> GetForUser(int userId)
    {
        await AddBearerTokenAsync();
        var jobApplicationDefaults = await _client.GetJobApplicationDefaultsByUserIdAsync(userId);
        return _mapper.Map<JobApplicationDefaultsModel>(jobApplicationDefaults);
    }
}
