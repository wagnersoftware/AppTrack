using AppTrack.Frontend.ApiService.Base;
using AppTrack.Frontend.ApiService.Contracts;
using AppTrack.Frontend.ApiService.Mappings;
using AppTrack.Frontend.Models;
using Microsoft.AspNetCore.Components.Forms;

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

    public Task<Response<FreelancerProfileModel>> UploadCvAsync(IBrowserFile file) =>
        TryExecuteAsync(async () =>
        {
            const long maxSize = 10L * 1024 * 1024;
#pragma warning disable S5693 // 10 MB is an intentional, documented limit for CV uploads
            await using var stream = file.OpenReadStream(maxAllowedSize: maxSize);
#pragma warning restore S5693
            var fileParameter = new FileParameter(stream, file.Name, file.ContentType);
            var dto = await _client.CvAsync(fileParameter);
            return dto.ToModel();
        });

    public Task<Response<bool>> DeleteCvAsync() =>
        TryExecuteAsync(async () =>
        {
            await _client.CvDELETEAsync();
            return true;
        });

    public Task<Response<bool>> DeleteProfileAsync() =>
        TryExecuteAsync(async () =>
        {
            await _client.ProfileDELETEAsync();
            return true;
        });
}
