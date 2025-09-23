

using AppTrack.Application.Contracts.Mediator;

namespace AppTrack.Application.Features.JobApplications.Queries.GetAllJobApplications;
public record GetJobApplicationsQuery: IRequest<List<JobApplicationDto>>
{
}

