using MediatR;

namespace AppTrack.Application.Features.JobApplications.Commands.DeleteJobApplication;

public class DeleteJobApplicationCommand: IRequest<Unit>
{
    public int Id { get; set; }
}
