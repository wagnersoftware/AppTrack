using System.Text.Json.Serialization;
using AppTrack.Application.Contracts.Mediator;
using AppTrack.Application.Features.FreelancerProfile.Dto;

namespace AppTrack.Application.Features.FreelancerProfile.Commands.DeleteCv;

public class DeleteCvCommand : IRequest<FreelancerProfileDto>, IUserScopedRequest
{
    [JsonIgnore]
    public string UserId { get; set; } = string.Empty;
}
