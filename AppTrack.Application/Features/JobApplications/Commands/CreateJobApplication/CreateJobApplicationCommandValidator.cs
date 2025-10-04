using AppTrack.Application.Contracts.Persistance;
using FluentValidation;

namespace AppTrack.Application.Features.JobApplications.Commands.CreateJobApplication;

public class CreateJobApplicationCommandValidator : AbstractValidator<CreateJobApplicationCommand>
{
    public CreateJobApplicationCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("{PropertyName} is required")
            .NotNull().WithMessage("{PropertyName} is required")
            .MaximumLength(50).WithMessage("{PropertyName} must be fewer than 50 characters");

        RuleFor(x => x.Position)
            .NotEmpty().WithMessage("{PropertyName} is required")
            .NotNull().WithMessage("{PropertyName} is required")
            .MaximumLength(30).WithMessage("{PropertyName} must be fewer than 30 characters");

        RuleFor(x => x.URL)
            .NotEmpty().WithMessage("{PropertyName} is required")
            .NotNull().WithMessage("{PropertyName} is required")
            .MaximumLength(500).WithMessage("{PropertyName} must be fewer than 500 characters");

        RuleFor(x => x.Status)
            .NotNull().WithMessage("{PropertyName} is required");
    }
}
