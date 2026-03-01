using AppTrack.Frontend.ApiService.Base;
using AppTrack.Frontend.ApiService.Contracts;
using AppTrack.Frontend.ApiService.Mappings;
using AppTrack.Frontend.Models;

namespace AppTrack.Frontend.ApiService.Services;

public class JobApplicationDefaultsService : BaseHttpService, IJobApplicationDefaultsService
{
    public JobApplicationDefaultsService(IClient client, ITokenStorage tokenStorage) : base(client, tokenStorage)
    {
    }

    public Task<Response<JobApplicationDefaultsModel>> GetForUserAsync(string userId) =>
        TryExecuteAsync(async () =>
        {
            var jobApplicationDefaults = await _client.JobApplicationDefaultsGETAsync(userId);
            return jobApplicationDefaults.ToModel();
        });

    public Task<Response<JobApplicationDefaultsModel>> UpdateAsync(int id, JobApplicationDefaultsModel jobApplicationDefaultsModel) =>
        TryExecuteAsync(async () =>
        {
            var dto = await _client.JobApplicationDefaultsPUTAsync(id, jobApplicationDefaultsModel.ToUpdateCommand());
            return dto.ToModel();
        });
}
