using AppTrack.Application.Contracts.Mediator;
using AppTrack.Application.Features.JobApplicationDefaults.Dto;
using AppTrack.Shared.Validation.Interfaces;

namespace AppTrack.Application.Features.JobApplicationDefaults.Commands.UpdateApplicationDefaults;

public class UpdateJobApplicationDefaultsCommand : IRequest<JobApplicationDefaultsDto>, IJobApplicationDefaultsValidatable, IUserScopedRequest
{
    public int Id { get; set; }

    public string UserId { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string Position { get; set; } = string.Empty;

    public string Location { get; set; } = string.Empty;
}
