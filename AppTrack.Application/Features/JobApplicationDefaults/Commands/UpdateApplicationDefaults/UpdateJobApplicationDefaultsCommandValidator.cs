using AppTrack.Application.Contracts.Persistance;
using FluentValidation;

namespace AppTrack.Application.Features.JobApplicationDefaults.Commands.UpdateApplicationDefaults;

public class UpdateJobApplicationDefaultsCommandValidator : AbstractValidator<UpdateJobApplicationDefaultsCommand>
{
    private readonly IJobApplicationDefaultsRepository _jobApplicationDefaultsRepository;

    public UpdateJobApplicationDefaultsCommandValidator(IJobApplicationDefaultsRepository jobApplicationDefaultsRepository)
    {
        _jobApplicationDefaultsRepository = jobApplicationDefaultsRepository;

        RuleFor(x => x.Id)
        .NotNull().WithMessage("{PropertyName} is required");

        RuleFor(x => x.UserId)
        .NotEmpty().WithMessage("{PropertyName} is required")
        .NotNull().WithMessage("{PropertyName} is required");

        RuleFor(x => x)
        .MustAsync(JobApplicationDefaultsExists)
        .WithMessage("Job application doesn't exist");

        RuleFor(x => x)
        .MustAsync(JobApplicationDefaultsExistsForUser)
        .WithMessage("The requested job application defaults don't match the use id");
    }

    private async Task<bool> JobApplicationDefaultsExists(UpdateJobApplicationDefaultsCommand command, CancellationToken token)
    {
        var jobApplicationDefault = await _jobApplicationDefaultsRepository.GetByIdAsync(command.Id!);
        return jobApplicationDefault != null;
    }

    private async Task<bool> JobApplicationDefaultsExistsForUser(UpdateJobApplicationDefaultsCommand command, CancellationToken token)
    {
        var jobApplicationDefault = await _jobApplicationDefaultsRepository.GetByIdAsync(command.Id!);
        return jobApplicationDefault?.UserId == command.UserId;
    }
}
