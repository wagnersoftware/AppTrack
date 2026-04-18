using AppTrack.Domain;
using AppTrack.Shared.Validation.Interfaces;
using FluentValidation;

namespace AppTrack.Shared.Validation.Validators;

public abstract class PromptBaseValidator<T> : AbstractValidator<T>
    where T : IPromptValidatable
{
    protected PromptBaseValidator()
    {
        RuleFor(x => x.Key)
            .NotEmpty().WithMessage("Key is required.")
            .MaximumLength(100).WithMessage("Key must not exceed 100 characters.")
            .Must(n => !n.Contains(' ')).WithMessage("A prompt key must not contain spaces.")
            .Must(n => !n.StartsWith(BuiltInParameterKeys.Prefix, StringComparison.OrdinalIgnoreCase))
                .WithMessage($"A prompt key must not start with '{BuiltInParameterKeys.Prefix}'.");

        RuleFor(x => x.PromptTemplate)
            .NotEmpty().WithMessage("Prompt template is required.");
    }
}
