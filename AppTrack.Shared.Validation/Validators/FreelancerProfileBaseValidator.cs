using AppTrack.Shared.Validation.Interfaces;
using FluentValidation;

namespace AppTrack.Shared.Validation.Validators;

public abstract class FreelancerProfileBaseValidator<T> : AbstractValidator<T>
    where T : IFreelancerProfileValidatable
{
    protected FreelancerProfileBaseValidator()
    {
        RuleFor(x => x.FirstName)
            .MaximumLength(100).WithMessage("{PropertyName} must not exceed 100 characters.")
            .When(x => !string.IsNullOrEmpty(x.FirstName));

        RuleFor(x => x.LastName)
            .MaximumLength(100).WithMessage("{PropertyName} must not exceed 100 characters.")
            .When(x => !string.IsNullOrEmpty(x.LastName));

        RuleFor(x => x.HourlyRate)
            .GreaterThan(0).WithMessage("{PropertyName} must be greater than 0.")
            .When(x => x.HourlyRate.HasValue);

        RuleFor(x => x.DailyRate)
            .GreaterThan(0).WithMessage("{PropertyName} must be greater than 0.")
            .When(x => x.DailyRate.HasValue);

        RuleFor(x => x.Skills)
            .MaximumLength(1000).WithMessage("{PropertyName} must not exceed 1000 characters.")
            .When(x => x.Skills != null);
    }
}
