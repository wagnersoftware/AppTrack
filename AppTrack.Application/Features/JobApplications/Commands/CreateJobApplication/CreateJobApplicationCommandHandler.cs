using AppTrack.Application.Contracts.Mediator;
using AppTrack.Application.Contracts.Persistance;
using AppTrack.Application.Exceptions;
using AppTrack.Application.Features.JobApplications.Dto;
using AppTrack.Application.Mappings;

namespace AppTrack.Application.Features.JobApplications.Commands.CreateJobApplication;

public class CreateJobApplicationCommandHandler : IRequestHandler<CreateJobApplicationCommand, JobApplicationDto>
{
    private readonly IJobApplicationRepository _jobApplicationRepository;

    public CreateJobApplicationCommandHandler(IJobApplicationRepository jobApplicationRepository)
    {
        _jobApplicationRepository = jobApplicationRepository;
    }

    public async Task<JobApplicationDto> Handle(CreateJobApplicationCommand request, CancellationToken cancellationToken)
    {
        var validator = new CreateJobApplicationCommandValidator();
        var validationResult = await validator.ValidateAsync(request);

        if (validationResult.Errors.Any())
        {
            throw new BadRequestException($"Invalid Request", validationResult);
        }

        var jobApplicationToCreate = request.ToDomain();

        await _jobApplicationRepository.CreateAsync(jobApplicationToCreate);

        return jobApplicationToCreate.ToDto();
    }
}
