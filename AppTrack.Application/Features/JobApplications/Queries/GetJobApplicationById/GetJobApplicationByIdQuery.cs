using AppTrack.Application.Features.JobApplications.Queries.GetAllJobApplications;
using MediatR;

namespace AppTrack.Application.Features.JobApplications.Queries.GetJobApplicationById;

public class GetJobApplicationByIdQuery : IRequest<JobApplicationDto>
{
    public int Id { get; set; }
}
