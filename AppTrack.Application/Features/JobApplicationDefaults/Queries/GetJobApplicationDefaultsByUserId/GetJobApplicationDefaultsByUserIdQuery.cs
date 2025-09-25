using AppTrack.Application.Contracts.Mediator;
using AppTrack.Application.Features.JobApplicationDefaults.Dto;

namespace AppTrack.Application.Features.JobApplicationDefaults.Queries.GetJobApplicationDefaultsByUserId
{
    public class GetJobApplicationDefaultsByUserIdQuery : IRequest<JobApplicationDefaultsDto>
    {
        public string UserId { get; set; } = string.Empty;
    }
}
