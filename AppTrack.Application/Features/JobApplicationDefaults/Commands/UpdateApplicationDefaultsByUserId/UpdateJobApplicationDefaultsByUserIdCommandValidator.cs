using AppTrack.Application.Contracts.Persistance;
using FluentValidation;

namespace AppTrack.Application.Features.JobApplicationDefaults.Commands.UpdateApplicationDefaultsByUserId;

public class UpdateJobApplicationDefaultsByUserIdCommandValidator : AbstractValidator<UpdateJobApplicationDefaultsByUserIdCommand>
{
    private readonly IJobApplicationDefaultsRepository _jobApplicationDefaultsRepository;

    public UpdateJobApplicationDefaultsByUserIdCommandValidator(IJobApplicationDefaultsRepository jobApplicationDefaultsRepository)
    {
        this._jobApplicationDefaultsRepository = jobApplicationDefaultsRepository;

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

    private async Task<bool> JobApplicationDefaultsExistsForUser(UpdateJobApplicationDefaultsByUserIdCommand command, CancellationToken token)
    {
        var jobApplicationDefault = await _jobApplicationDefaultsRepository.GetByUserIdAsync(command.UserId!);
        return jobApplicationDefault != null;
    }

    private async Task<bool> JobApplicationDefaultsExists(UpdateJobApplicationDefaultsByUserIdCommand command, CancellationToken token)
    {
        var jobApplicationDefault = await _jobApplicationDefaultsRepository.GetByIdAsync(command.Id!);
        return jobApplicationDefault != null;
    }
}
