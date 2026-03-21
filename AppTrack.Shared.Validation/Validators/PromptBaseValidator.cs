using AppTrack.Shared.Validation.Interfaces;
using FluentValidation;

namespace AppTrack.Shared.Validation.Validators;

public abstract class PromptBaseValidator<T> : AbstractValidator<T>
    where T : IPromptValidatable
{
    protected PromptBaseValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(100).WithMessage("Name must not exceed 100 characters.");

        RuleFor(x => x.PromptTemplate)
            .NotEmpty().WithMessage("Prompt template is required.");
    }
}
