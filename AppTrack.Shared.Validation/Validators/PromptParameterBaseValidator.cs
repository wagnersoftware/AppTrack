using AppTrack.Domain;
using AppTrack.Shared.Validation.Interfaces;
using FluentValidation;

namespace AppTrack.Shared.Validation.Validators;

public abstract class PromptParameterBaseValidator<T> : AbstractValidator<T>
    where T : IPromptParameterValidatable
{
    protected PromptParameterBaseValidator()
    {
        RuleFor(x => x.Key)
            .NotEmpty().WithMessage("Key is required.")
            .MaximumLength(50).WithMessage("Key must not exceed 50 characters.")
            .Must(k => !k.StartsWith(BuiltInParameterKeys.Prefix, StringComparison.OrdinalIgnoreCase))
                .WithMessage($"A parameter key must not start with '{BuiltInParameterKeys.Prefix}'.");

        RuleFor(x => x.Value)
            .NotEmpty().WithMessage("Value is required.")
            .MaximumLength(500).WithMessage("Value must not exceed 500 characters.");
    }
}
