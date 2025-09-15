using AppTrack.Application.Contracts.Logging;
using AppTrack.Application.Contracts.Persistance;
using AppTrack.Application.Exceptions;
using MediatR;

namespace AppTrack.Application.Features.JobApplications.Commands.DeleteJobApplication;

public class DeleteJobApplicationCommandHandler: IRequestHandler<DeleteJobApplicationCommand, Unit>
{
    private readonly IJobApplicationRepository _jobApplicationRepository;
    private readonly IAppLogger<DeleteJobApplicationCommandHandler> _logger;

    public DeleteJobApplicationCommandHandler(IJobApplicationRepository jobApplicationRepository, IAppLogger<DeleteJobApplicationCommandHandler> logger)
    {
        this._jobApplicationRepository = jobApplicationRepository;
        this._logger = logger;
    }

    public async Task<Unit> Handle(DeleteJobApplicationCommand request, CancellationToken cancellationToken)
    {
        var jobApplicationToDelete = await _jobApplicationRepository.GetByIdAsync(request.Id);

        if (jobApplicationToDelete == null)
        {
            _logger.LogWarning("Validation errors in {0} - {1}", nameof(Domain.JobApplication), request.Id);
            throw new NotFoundException(nameof(Domain.JobApplication), request.Id);
        }

        await _jobApplicationRepository.DeleteAsync(jobApplicationToDelete);

        return Unit.Value;
    }
}
