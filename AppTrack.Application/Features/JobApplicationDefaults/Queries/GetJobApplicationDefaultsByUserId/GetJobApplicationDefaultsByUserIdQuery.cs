using System.Text.Json.Serialization;
using AppTrack.Application.Contracts.Mediator;
using AppTrack.Application.Features.JobApplicationDefaults.Dto;

namespace AppTrack.Application.Features.JobApplicationDefaults.Queries.GetJobApplicationDefaultsByUserId
{
    public class GetJobApplicationDefaultsByUserIdQuery : IRequest<JobApplicationDefaultsDto>, IUserScopedRequest
    {
        [JsonIgnore]
        public string UserId { get; set; } = string.Empty;
    }
}
