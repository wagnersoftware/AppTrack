using AppTrack.Shared.Validation.Validators;
using FluentValidation;

namespace AppTrack.Frontend.Models.Validators;

public class PromptModelValidator : PromptBaseValidator<PromptModel>
{
    public PromptModelValidator()
    {
        RuleFor(x => x.Name)
            .Must((model, name) => model.SiblingPrompts == null ||
                                   !model.SiblingPrompts.Any(p => p.TempId != model.TempId &&
                                                                   string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase)))
            .WithMessage("A prompt with this name already exists.");
    }
}
