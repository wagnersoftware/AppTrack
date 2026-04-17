using AppTrack.Application.Contracts.Mediator;
using AppTrack.Application.Contracts.Persistance;
using AppTrack.Application.Exceptions;
using AppTrack.Application.Features.JobApplications.Dto;
using AppTrack.Application.Mappings;
using FluentValidation;

namespace AppTrack.Application.Features.JobApplications.Commands.CreateJobApplication;

public class CreateJobApplicationCommandHandler : IRequestHandler<CreateJobApplicationCommand, JobApplicationDto>
{
    private readonly IJobApplicationRepository _jobApplicationRepository;
    private readonly IValidator<CreateJobApplicationCommand> _validator;

    public CreateJobApplicationCommandHandler(IJobApplicationRepository jobApplicationRepository, IValidator<CreateJobApplicationCommand> validator)
    {
        _jobApplicationRepository = jobApplicationRepository;
        _validator = validator;
    }

    public async Task<JobApplicationDto> Handle(CreateJobApplicationCommand request, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(request);

        if (validationResult.Errors.Any())
        {
            throw new BadRequestException($"Invalid Request", validationResult);
        }

        var jobApplicationToCreate = request.ToDomain();

        await _jobApplicationRepository.CreateAsync(jobApplicationToCreate);

        return jobApplicationToCreate.ToDto();
    }
}
