using FluentValidation;

namespace AppTrack.Application.Features.JobApplications.Commands.CreateJobApplication;

public class CreateJobApplicationCommandValidator : AbstractValidator<CreateJobApplicationCommand>
{
    public CreateJobApplicationCommandValidator()
    {
        RuleFor(x => x.Name)
            .MaximumLength(200).WithMessage("{PropertyName}} must not exceed 200 characters.")
            .NotEmpty().WithMessage("{PropertyName} is required")
            .NotNull().WithMessage("{PropertyName} is required");

        RuleFor(x => x.Position)
            .MaximumLength(200).WithMessage("{PropertyName}} must not exceed 200 characters.")
            .NotEmpty().WithMessage("{PropertyName} is required")
            .NotNull().WithMessage("{PropertyName} is required");

        RuleFor(x => x.URL)
            .MaximumLength(1000).WithMessage("{PropertyName}} must not exceed 200 characters.")
            .NotEmpty().WithMessage("{PropertyName} is required")
            .NotNull().WithMessage("{PropertyName} is required")
            .Must(BeAValidUrl).WithMessage("{PropertyName} must be a valid URL");

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("{PropertyName} is required")
            .NotNull().WithMessage("{PropertyName} is required");

        RuleFor(x => x.JobDescription)
            .NotEmpty().WithMessage("{PropertyName} is required")
            .NotNull().WithMessage("{PropertyName} is required");

        RuleFor(x => x.Location)
            .MaximumLength(200).WithMessage("{PropertyName}} must not exceed 200 characters.")
            .NotEmpty().WithMessage("{PropertyName} is required")
            .NotNull().WithMessage("{PropertyName} is required");

        RuleFor(x => x.ContactPerson)
            .MaximumLength(200).WithMessage("ContactPerson must not exceed 200 characters.")
            .NotEmpty().WithMessage("{PropertyName} is required")
            .NotNull().WithMessage("{PropertyName} is required");

        RuleFor(x => x.Status)
            .NotNull().WithMessage("{PropertyName} is required");

        RuleFor(x => x.StartDate)
            .NotEmpty().WithMessage("{PropertyName} is required")
            .NotNull().WithMessage("{PropertyName} is required");

        RuleFor(x => x.DurationInMonths)
            .Must(value => string.IsNullOrEmpty(value) || int.TryParse(value, out _))
            .WithMessage("{PropertyName} must be a valid number.");
    }

    private static bool BeAValidUrl(string url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out var uriResult)
               && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
    }
}
