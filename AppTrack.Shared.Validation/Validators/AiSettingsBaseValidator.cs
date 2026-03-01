using AppTrack.Shared.Validation.Interfaces;
using FluentValidation;

namespace AppTrack.Shared.Validation.Validators;

public abstract class AiSettingsBaseValidator<T> : AbstractValidator<T>
    where T : IAiSettingsValidatable
{
    protected AiSettingsBaseValidator()
    {
        RuleFor(x => x.ApiKey)
            .MaximumLength(200).WithMessage("ApiKey must not exceed 200 characters.");

        RuleForEach(x => x.PromptParameter)
            .SetValidator(new PromptParameterItemValidator());

        RuleFor(x => x.PromptParameter)
            .Must(HaveUniqueKeys)
            .WithMessage("Each prompt parameter key must be unique.");
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

    private sealed class PromptParameterItemValidator : PromptParameterBaseValidator<IPromptParameterValidatable>
    {
        public PromptParameterItemValidator() : base() { }
    }
}
