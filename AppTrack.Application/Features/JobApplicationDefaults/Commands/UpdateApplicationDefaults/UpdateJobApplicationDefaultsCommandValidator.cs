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
        .GreaterThan(0).WithMessage("{PropertyName} must be greater than 0.")
        .NotEmpty().WithMessage("{PropertyName} is required");

        RuleFor(x => x.UserId)
        .NotEmpty().WithMessage("UserId is required")
        .Matches("^[a-zA-Z0-9\\-]+$").WithMessage("UserId contains invalid characters.");

        RuleFor(x => x)
        .CustomAsync(async (command, context, cancellationToken) =>
        {
            var jobApplicationDefaults = await _jobApplicationDefaultsRepository.GetByIdAsync(command.Id);
            if (jobApplicationDefaults is null)
            {
                context.AddFailure("Id", "Job Application Defaults not found.");
                return;
            }

            if (jobApplicationDefaults.UserId != command.UserId)
            {
                context.AddFailure("UserId", "Job Application Defaults not assigned to this user.");
            }
        });

        RuleFor(x => x.Name)
            .MaximumLength(200).WithMessage("Name must not exceed 200 characters.");

        RuleFor(x => x.Position)
            .MaximumLength(200).WithMessage("Name must not exceed 200 characters.");

        RuleFor(x => x.Location)
            .MaximumLength(200).WithMessage("Name must not exceed 200 characters.");
    }
}
