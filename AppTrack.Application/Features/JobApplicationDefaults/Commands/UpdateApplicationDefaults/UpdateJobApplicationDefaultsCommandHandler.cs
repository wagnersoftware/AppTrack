using AppTrack.Application.Contracts.Mediator;
using AppTrack.Application.Contracts.Persistance;
using AppTrack.Application.Exceptions;
using AppTrack.Application.Mappings;
using AppTrack.Application.Shared;

namespace AppTrack.Application.Features.JobApplicationDefaults.Commands.UpdateApplicationDefaults;

public class UpdateJobApplicationDefaultsCommandHandler : IRequestHandler<UpdateJobApplicationDefaultsCommand, Unit>
{
    private readonly IJobApplicationDefaultsRepository _jobApplicationDefaultsRepository;

    public UpdateJobApplicationDefaultsCommandHandler(IJobApplicationDefaultsRepository jobApplicationDefaultsRepository)
    {
        _jobApplicationDefaultsRepository = jobApplicationDefaultsRepository;
    }

    public async Task<Unit> Handle(UpdateJobApplicationDefaultsCommand request, CancellationToken cancellationToken)
    {
        var validator = new UpdateJobApplicationDefaultsCommandValidator(_jobApplicationDefaultsRepository);
        var validationResult = await validator.ValidateAsync(request);

        if (validationResult.Errors.Any())
        {
            throw new BadRequestException($"Invalid update job application defaults request", validationResult);
        }

        var jobApplicationDefaultsToUpdate = await _jobApplicationDefaultsRepository.GetByIdAsync(request.Id);
        request.ApplyTo(jobApplicationDefaultsToUpdate!);
        await _jobApplicationDefaultsRepository.UpdateAsync(jobApplicationDefaultsToUpdate!);

        return Unit.Value;
    }
}
