using AppTrack.Application.Contracts.Mediator;
using AppTrack.Application.Features.JobApplications.Dto;

namespace AppTrack.Application.Features.JobApplications.Queries.GetAllJobApplicationsForUser;
public record GetJobApplicationsForUserQuery : IRequest<List<JobApplicationDto>>
{
    public string UserId { get; set; } = string.Empty;
}

