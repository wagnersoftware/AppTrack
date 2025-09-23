

using AppTrack.Application.Contracts.Mediator;
using AppTrack.Application.Features.JobApplications.Dto;

namespace AppTrack.Application.Features.JobApplications.Queries.GetAllJobApplications;
public record GetJobApplicationsQuery: IRequest<List<JobApplicationDto>>
{
}

