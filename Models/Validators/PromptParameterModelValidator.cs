using AppTrack.Shared.Validation.Validators;
using FluentValidation;

namespace AppTrack.Frontend.Models.Validators;

public class PromptParameterModelValidator : PromptParameterBaseValidator<PromptParameterModel>
{
    public PromptParameterModelValidator()
    {
        RuleFor(x => x.Key)
            .Must((model, key) => model.ParentCollection == null ||
                                  !model.ParentCollection.Any(p => p.TempId != model.TempId &&
                                                                    string.Equals(p.Key, key, StringComparison.OrdinalIgnoreCase)))
            .WithMessage("A prompt parameter with this key already exists.");
    }
}
