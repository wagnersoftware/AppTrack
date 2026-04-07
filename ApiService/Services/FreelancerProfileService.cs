using AppTrack.Frontend.ApiService.Base;
using AppTrack.Frontend.ApiService.Contracts;
using AppTrack.Frontend.ApiService.Mappings;
using AppTrack.Frontend.Models;

namespace AppTrack.Frontend.ApiService.Services;

public class FreelancerProfileService : BaseHttpService, IFreelancerProfileService
{
    public FreelancerProfileService(IClient client) : base(client)
    {
    }

    public Task<Response<FreelancerProfileModel>> GetProfileAsync() =>
        TryExecuteAsync(async () =>
        {
            var dto = await _client.ProfileGETAsync();
            return dto.ToModel();
        });

    public Task<Response<FreelancerProfileModel>> UpsertProfileAsync(FreelancerProfileModel model) =>
        TryExecuteAsync(async () =>
        {
            var command = model.ToUpsertCommand();
            var dto = await _client.ProfilePUTAsync(command);
            return dto.ToModel();
        });
}
