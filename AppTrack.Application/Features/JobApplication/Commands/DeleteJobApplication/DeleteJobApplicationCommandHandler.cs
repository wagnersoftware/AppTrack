using AppTrack.Application.Contracts.Persistance;
using AppTrack.Application.Exceptions;
using AutoMapper;
using MediatR;

namespace AppTrack.Application.Features.JobApplication.Commands.DeleteJobApplication;

public class DeleteJobApplicationCommandHandler: IRequestHandler<DeleteJobApplicationCommand, Unit>
{
    private readonly IJobApplicationRepository _jobApplicationRepository;

    public DeleteJobApplicationCommandHandler(IJobApplicationRepository jobApplicationRepository)
    {
        _jobApplicationRepository = jobApplicationRepository;
    }

    public async Task<Unit> Handle(DeleteJobApplicationCommand request, CancellationToken cancellationToken)
    {
        var jobApplicationToDelete = await _jobApplicationRepository.GetByIdAsync(request.Id);

        if (jobApplicationToDelete == null)
        {
            throw new NotFoundException(nameof(JobApplication), request.Id);
        }

        await _jobApplicationRepository.DeleteAsync(jobApplicationToDelete);

        return Unit.Value;
    }
}
