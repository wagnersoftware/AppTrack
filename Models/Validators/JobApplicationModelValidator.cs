using AppTrack.Shared.Validation.Validators;
using FluentValidation;

namespace AppTrack.Frontend.Models.Validators;

public class JobApplicationModelValidator : JobApplicationBaseValidator<JobApplicationModel>
{
    public JobApplicationModelValidator()
    {
        RuleFor(x => x.Status)
            .NotNull().WithMessage("{PropertyName} is required");

        RuleFor(x => x.StartDate)
            .NotEqual(DateOnly.MinValue).WithMessage("{PropertyName} is required");
    }
}
