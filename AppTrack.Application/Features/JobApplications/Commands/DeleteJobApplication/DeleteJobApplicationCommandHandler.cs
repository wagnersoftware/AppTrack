using AppTrack.Application.Contracts.Logging;
using AppTrack.Application.Contracts.Mediator;
using AppTrack.Application.Contracts.Persistance;
using AppTrack.Application.Exceptions;
using AppTrack.Application.Shared;

namespace AppTrack.Application.Features.JobApplications.Commands.DeleteJobApplication;

public class DeleteJobApplicationCommandHandler : IRequestHandler<DeleteJobApplicationCommand, Unit>
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
        var validator = new DeleteJobApplicationCommandValidator(_jobApplicationRepository);
        var validationResult = await validator.ValidateAsync(request);

        if (validationResult.Errors.Any())
        {
            throw new BadRequestException($"Invalid delete request", validationResult);
        }

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
