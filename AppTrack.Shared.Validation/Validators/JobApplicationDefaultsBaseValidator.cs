using AppTrack.Shared.Validation.Interfaces;
using FluentValidation;

namespace AppTrack.Shared.Validation.Validators;

public abstract class JobApplicationDefaultsBaseValidator<T> : AbstractValidator<T>
    where T : IJobApplicationDefaultsValidatable
{
    protected JobApplicationDefaultsBaseValidator()
    {
        RuleFor(x => x.Name)
            .MaximumLength(200).WithMessage("{PropertyName} must not exceed 200 characters.");

        RuleFor(x => x.Position)
            .MaximumLength(200).WithMessage("{PropertyName} must not exceed 200 characters.");

        RuleFor(x => x.Location)
            .MaximumLength(200).WithMessage("{PropertyName} must not exceed 200 characters.");
    }
}
