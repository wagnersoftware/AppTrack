using System.Text.Json.Serialization;
using AppTrack.Application.Contracts.Mediator;
using AppTrack.Application.Features.FreelancerProfile.Dto;

namespace AppTrack.Application.Features.FreelancerProfile.Queries.GetFreelancerProfile;

public class GetFreelancerProfileQuery : IRequest<FreelancerProfileDto>, IUserScopedRequest
{
    [JsonIgnore]
    public string UserId { get; set; } = string.Empty;
}
