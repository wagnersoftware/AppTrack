using System.Text.Json.Serialization;
using AppTrack.Application.Contracts.Mediator;
using AppTrack.Application.Features.JobApplications.Dto;

namespace AppTrack.Application.Features.JobApplications.Queries.GetAllJobApplicationsForUser;

public class GetJobApplicationsForUserQuery : IRequest<List<JobApplicationDto>>, IUserScopedRequest
{
    [JsonIgnore]
    public string UserId { get; set; } = string.Empty;
}
