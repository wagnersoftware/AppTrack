

using AppTrack.Application.Contracts.Mediator;
using AppTrack.Application.Shared;

namespace AppTrack.Application.Features.JobApplications.Commands.DeleteJobApplication;

public class DeleteJobApplicationCommand : IRequest<Unit>
{
    public int Id { get; set; }
}
