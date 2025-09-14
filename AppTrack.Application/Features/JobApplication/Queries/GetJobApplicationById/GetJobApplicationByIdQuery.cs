using AppTrack.Application.Features.JobApplication.Queries.GetAllJobApplications;
using MediatR;

namespace AppTrack.Application.Features.JobApplication.Queries.GetJobApplicationById;

public class GetJobApplicationByIdQuery : IRequest<JobApplicationDto>
{
    public int Id { get; set; }
}
