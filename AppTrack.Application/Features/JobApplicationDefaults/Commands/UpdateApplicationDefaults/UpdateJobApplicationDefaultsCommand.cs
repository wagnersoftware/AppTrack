using AppTrack.Application.Contracts.Mediator;
using AppTrack.Application.Shared;

namespace AppTrack.Application.Features.JobApplicationDefaults.Commands.UpdateApplicationDefaults;

public class UpdateJobApplicationDefaultsCommand : IRequest<Unit>
{
    public int Id { get; set; }

    public string UserId { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string Position { get; set; } = string.Empty;

    public string Location { get; set; } = string.Empty;
}
