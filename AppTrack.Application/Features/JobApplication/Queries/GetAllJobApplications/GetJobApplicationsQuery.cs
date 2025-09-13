using MediatR;

namespace AppTrack.Application.Features.JobApplication.Queries.GetAllJobApplications;
public record GetJobApplicationsQuery: IRequest<List<JobApplicationDto>>
{
}

