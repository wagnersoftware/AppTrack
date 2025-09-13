using MediatR;

namespace AppTrack.Application.Features.JobApplication.Commands.DeleteJobApplication;

public class DeleteJobApplicationCommand: IRequest<Unit>
{
    public int Id { get; set; }
}
