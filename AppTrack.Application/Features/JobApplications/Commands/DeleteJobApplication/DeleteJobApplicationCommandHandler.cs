using AppTrack.Application.Contracts.Mediator;
using AppTrack.Application.Contracts.Persistance;
using AppTrack.Application.Exceptions;
using AppTrack.Application.Shared;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace AppTrack.Application.Features.JobApplications.Commands.DeleteJobApplication;

public class DeleteJobApplicationCommandHandler : IRequestHandler<DeleteJobApplicationCommand, Unit>
{
    private readonly IJobApplicationRepository _jobApplicationRepository;
    private readonly ILogger<DeleteJobApplicationCommandHandler> _logger;
    private readonly IValidator<DeleteJobApplicationCommand> _validator;

    public DeleteJobApplicationCommandHandler(IJobApplicationRepository jobApplicationRepository, ILogger<DeleteJobApplicationCommandHandler> logger, IValidator<DeleteJobApplicationCommand> validator)
    {
        this._jobApplicationRepository = jobApplicationRepository;
        this._logger = logger;
        _validator = validator;
    }

    public async Task<Unit> Handle(DeleteJobApplicationCommand request, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(request);

        if (validationResult.Errors.Any())
        {
            throw new BadRequestException($"Invalid delete request", validationResult);
        }

        var jobApplicationToDelete = await _jobApplicationRepository.GetByIdAsync(request.Id);

        if (jobApplicationToDelete == null)
        {
            _logger.LogWarning("Job application not found: {ResourceType} {ResourceId}", nameof(Domain.JobApplication), request.Id);
            throw new NotFoundException(nameof(Domain.JobApplication), request.Id);
        }

        await _jobApplicationRepository.DeleteAsync(jobApplicationToDelete);

        return Unit.Value;
    }
}
