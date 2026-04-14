using AppTrack.Frontend.ApiService.Base;
using AppTrack.Frontend.Models;
using Microsoft.AspNetCore.Components.Forms;

namespace AppTrack.Frontend.ApiService.Contracts;

public interface IFreelancerProfileService
{
    Task<Response<FreelancerProfileModel>> GetProfileAsync();
    Task<Response<FreelancerProfileModel>> UpsertProfileAsync(FreelancerProfileModel model);
    Task<Response<FreelancerProfileModel>> UploadCvAsync(IBrowserFile file);
    Task<Response<FreelancerProfileModel>> DeleteCvAsync();
    Task<Response<bool>> DeleteProfileAsync();
}
