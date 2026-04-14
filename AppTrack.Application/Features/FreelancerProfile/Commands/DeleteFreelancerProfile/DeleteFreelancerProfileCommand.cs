using System.Text.Json.Serialization;
using AppTrack.Application.Contracts.Mediator;
using AppTrack.Application.Shared;

namespace AppTrack.Application.Features.FreelancerProfile.Commands.DeleteFreelancerProfile;

public class DeleteFreelancerProfileCommand : IRequest<Unit>, IUserScopedRequest
{
    [JsonIgnore]
    public string UserId { get; set; } = string.Empty;
}
