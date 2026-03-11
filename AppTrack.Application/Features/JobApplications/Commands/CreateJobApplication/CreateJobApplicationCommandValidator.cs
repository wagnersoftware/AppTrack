using AppTrack.Shared.Validation.Validators;
using FluentValidation;

namespace AppTrack.Application.Features.JobApplications.Commands.CreateJobApplication;

public class CreateJobApplicationCommandValidator : JobApplicationBaseValidator<CreateJobApplicationCommand>
{
    public CreateJobApplicationCommandValidator()
    {
        RuleFor(x => x.Status)
            .NotNull().WithMessage("{PropertyName} is required");

        RuleFor(x => x.StartDate)
            .NotNull().WithMessage("{PropertyName} is required")
            .NotEmpty().WithMessage("{PropertyName} is required");
    }
}
