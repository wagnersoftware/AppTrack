using AppTrack.Shared.Validation.Validators;
using FluentValidation;

namespace AppTrack.Frontend.Models.Validators;

public class PromptModelValidator : PromptBaseValidator<PromptModel>
{
    public PromptModelValidator()
    {
        RuleFor(x => x.Key)
            .Must((model, key) => model.SiblingPrompts == null ||
                                  !model.SiblingPrompts.Any(p => p.TempId != model.TempId &&
                                                                  string.Equals(p.Key, key, StringComparison.OrdinalIgnoreCase)))
            .WithMessage("A prompt with this key already exists.");
    }
}
