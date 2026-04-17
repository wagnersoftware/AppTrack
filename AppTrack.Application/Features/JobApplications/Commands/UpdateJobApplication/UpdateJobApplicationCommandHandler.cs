using AppTrack.Application.Contracts.Mediator;
using AppTrack.Application.Contracts.Persistance;
using AppTrack.Application.Exceptions;
using AppTrack.Application.Features.JobApplications.Dto;
using AppTrack.Application.Mappings;
using FluentValidation;

namespace AppTrack.Application.Features.JobApplications.Commands.UpdateJobApplication;

public class UpdateJobApplicationCommandHandler : IRequestHandler<UpdateJobApplicationCommand, JobApplicationDto>
{
    private readonly IJobApplicationRepository _jobApplicationRepository;
    private readonly IValidator<UpdateJobApplicationCommand> _validator;

    public UpdateJobApplicationCommandHandler(IJobApplicationRepository jobApplicationRepository, IValidator<UpdateJobApplicationCommand> validator)
    {
        _jobApplicationRepository = jobApplicationRepository;
        _validator = validator;
    }

    public async Task<JobApplicationDto> Handle(UpdateJobApplicationCommand request, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(request);

        if (validationResult.Errors.Any())
        {
            throw new BadRequestException($"Invalid JobApplication", validationResult);
        }

        var jobApplicationToUpdate = await _jobApplicationRepository.GetByIdAsync(request.Id);

        if (jobApplicationToUpdate == null)
        {
            throw new NotFoundException("Job Application", request.Id);
        }

        request.ApplyTo(jobApplicationToUpdate);

        await _jobApplicationRepository.UpdateAsync(jobApplicationToUpdate);

        return jobApplicationToUpdate.ToDto();
    }
}
