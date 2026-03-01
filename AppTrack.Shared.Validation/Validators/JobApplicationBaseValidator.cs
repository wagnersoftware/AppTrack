using AppTrack.Shared.Validation.Interfaces;
using FluentValidation;

namespace AppTrack.Shared.Validation.Validators;

public abstract class JobApplicationBaseValidator<T> : AbstractValidator<T>
    where T : IJobApplicationValidatable
{
    protected JobApplicationBaseValidator()
    {
        RuleFor(x => x.Name)
            .NotNull().WithMessage("{PropertyName} is required")
            .NotEmpty().WithMessage("{PropertyName} is required")
            .MaximumLength(200).WithMessage("{PropertyName} must not exceed 200 characters.");

        RuleFor(x => x.Position)
            .NotNull().WithMessage("{PropertyName} is required")
            .NotEmpty().WithMessage("{PropertyName} is required")
            .MaximumLength(200).WithMessage("{PropertyName} must not exceed 200 characters.");

        RuleFor(x => x.URL)
            .NotNull().WithMessage("{PropertyName} is required")
            .NotEmpty().WithMessage("{PropertyName} is required")
            .MaximumLength(1000).WithMessage("{PropertyName} must not exceed 1000 characters.")
            .Must(BeAValidUrl).WithMessage("{PropertyName} must be a valid URL.");

        RuleFor(x => x.JobDescription)
            .NotNull().WithMessage("{PropertyName} is required")
            .NotEmpty().WithMessage("{PropertyName} is required");

        RuleFor(x => x.Location)
            .NotNull().WithMessage("{PropertyName} is required")
            .NotEmpty().WithMessage("{PropertyName} is required")
            .MaximumLength(200).WithMessage("{PropertyName} must not exceed 200 characters.");

        RuleFor(x => x.ContactPerson)
            .NotNull().WithMessage("{PropertyName} is required")
            .NotEmpty().WithMessage("{PropertyName} is required")
            .MaximumLength(200).WithMessage("{PropertyName} must not exceed 200 characters.");

        RuleFor(x => x.DurationInMonths)
            .Cascade(CascadeMode.Stop)
            .MaximumLength(10).WithMessage("{PropertyName} must not exceed 10 characters.")
            .Must(value => string.IsNullOrEmpty(value) || int.TryParse(value, out _))
            .WithMessage("{PropertyName} must be a valid number.")
            .Must(value => string.IsNullOrEmpty(value) || (int.TryParse(value, out var months) && months > 0 && months <= 120))
            .WithMessage("{PropertyName} must be between 1 and 120.");
    }

    private static bool BeAValidUrl(string url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out var uriResult)
               && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
    }
}
