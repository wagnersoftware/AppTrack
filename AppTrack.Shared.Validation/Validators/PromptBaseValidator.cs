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
            .MaximumLength(100).WithMessage("Name must not exceed 100 characters.")
            .Must(n => !n.Contains(' ')).WithMessage("A prompt name must not contain spaces.")
            .Must(n => !n.StartsWith("builtIn_", StringComparison.OrdinalIgnoreCase))
                .WithMessage("A prompt name must not start with 'builtIn_'.");

        RuleFor(x => x.PromptTemplate)
            .NotEmpty().WithMessage("Prompt template is required.");
    }
}
