using AppTrack.Frontend.ApiService.Base;
using AppTrack.Frontend.Models;

namespace AppTrack.Frontend.ApiService.Contracts;

public interface IFreelancerProfileService
{
    Task<Response<FreelancerProfileModel>> GetProfileAsync();
    Task<Response<FreelancerProfileModel>> UpsertProfileAsync(FreelancerProfileModel model);
}
