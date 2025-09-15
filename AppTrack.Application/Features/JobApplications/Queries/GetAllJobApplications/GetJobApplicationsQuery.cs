using MediatR;

namespace AppTrack.Application.Features.JobApplications.Queries.GetAllJobApplications;
public record GetJobApplicationsQuery: IRequest<List<JobApplicationDto>>
{
}

