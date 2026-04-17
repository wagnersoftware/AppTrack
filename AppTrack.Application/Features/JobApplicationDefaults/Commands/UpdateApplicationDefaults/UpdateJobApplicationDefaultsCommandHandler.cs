using AppTrack.Application.Contracts.Mediator;
using AppTrack.Application.Contracts.Persistance;
using AppTrack.Application.Exceptions;
using AppTrack.Application.Features.JobApplicationDefaults.Dto;
using AppTrack.Application.Mappings;
using FluentValidation;

namespace AppTrack.Application.Features.JobApplicationDefaults.Commands.UpdateApplicationDefaults;

public class UpdateJobApplicationDefaultsCommandHandler : IRequestHandler<UpdateJobApplicationDefaultsCommand, JobApplicationDefaultsDto>
{
    private readonly IJobApplicationDefaultsRepository _jobApplicationDefaultsRepository;
    private readonly IValidator<UpdateJobApplicationDefaultsCommand> _validator;

    public UpdateJobApplicationDefaultsCommandHandler(IJobApplicationDefaultsRepository jobApplicationDefaultsRepository, IValidator<UpdateJobApplicationDefaultsCommand> validator)
    {
        _jobApplicationDefaultsRepository = jobApplicationDefaultsRepository;
        _validator = validator;
    }

    public async Task<JobApplicationDefaultsDto> Handle(UpdateJobApplicationDefaultsCommand request, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(request);

        if (validationResult.Errors.Any())
        {
            throw new BadRequestException($"Invalid update job application defaults request", validationResult);
        }

        var jobApplicationDefaultsToUpdate = await _jobApplicationDefaultsRepository.GetByIdAsync(request.Id);
        request.ApplyTo(jobApplicationDefaultsToUpdate!);
        await _jobApplicationDefaultsRepository.UpdateAsync(jobApplicationDefaultsToUpdate!);

        return jobApplicationDefaultsToUpdate!.ToDto();
    }
}
