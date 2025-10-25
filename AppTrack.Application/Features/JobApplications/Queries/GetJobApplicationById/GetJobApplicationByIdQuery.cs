using AppTrack.Application.Contracts.Mediator;
using AppTrack.Application.Features.JobApplications.Dto;

namespace AppTrack.Application.Features.JobApplications.Queries.GetJobApplicationById;

public class GetJobApplicationByIdQuery : IRequest<JobApplicationDto>
{
    public int Id { get; set; }

    public string UserId { get; set; } = string.Empty;
}
