using AppTrack.Shared.Validation.Interfaces;
using FluentValidation;

namespace AppTrack.Shared.Validation.Validators;

public abstract class AiSettingsBaseValidator<T> : AbstractValidator<T>
    where T : IAiSettingsValidatable
{
    protected AiSettingsBaseValidator()
    {
        // Existing PromptParameter rules — unchanged
        RuleForEach(x => x.PromptParameter)
            .SetValidator(new PromptParameterItemValidator());

        RuleFor(x => x.PromptParameter)
            .Must(HaveUniqueKeys)
            .WithMessage("Each prompt parameter key must be unique.");

        // New Prompts rules
        RuleForEach(x => x.Prompts)
            .SetValidator(new PromptItemValidator());

        RuleFor(x => x.Prompts)
            .Must(HaveUniqueKeys)
            .WithMessage("Each prompt key must be unique.");
    }

    private static bool HaveUniqueKeys(IEnumerable<IPromptParameterValidatable> parameters)
    {
        var list = parameters?.ToList();
        if (list is null || list.Count == 0)
            return true;

        return list.Select(p => p.Key)
                   .GroupBy(k => k, StringComparer.OrdinalIgnoreCase)
                   .All(g => g.Count() == 1);
    }

    private static bool HaveUniqueKeys(IEnumerable<IPromptValidatable> prompts)
    {
        var list = prompts?.ToList();
        if (list is null || list.Count == 0)
            return true;

        return list.Select(p => p.Key)
                   .GroupBy(k => k, StringComparer.OrdinalIgnoreCase)
                   .All(g => g.Count() == 1);
    }

    private sealed class PromptParameterItemValidator : PromptParameterBaseValidator<IPromptParameterValidatable>
    {
        public PromptParameterItemValidator() : base() { }
    }

    private sealed class PromptItemValidator : PromptBaseValidator<IPromptValidatable>
    {
        public PromptItemValidator() : base() { }
    }
}
