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

    public Task<Response<FreelancerProfileDto>> GetProfileAsync() =>
        TryExecuteAsync(async () =>
        {
            var dto = await _client.ProfileGETAsync();
            return dto;
        });

    public Task<Response<FreelancerProfileDto>> UpsertProfileAsync(FreelancerProfileModel model) =>
        TryExecuteAsync(async () =>
        {
            var command = model.ToUpsertCommand();
            var dto = await _client.ProfilePUTAsync(command);
            return dto;
        });
}
