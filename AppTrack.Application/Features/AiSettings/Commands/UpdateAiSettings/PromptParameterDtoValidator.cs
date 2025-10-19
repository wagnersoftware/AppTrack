using AppTrack.Application.Features.AiSettings.Dto;
using FluentValidation;

namespace AppTrack.Application.Features.AiSettings.Commands.UpdateAiSettings;

public class PromptParameterDtoValidator : AbstractValidator<PromptParameterDto>
{
    public PromptParameterDtoValidator()
    {
        RuleFor(x => x.Key)
            .NotEmpty().WithMessage("Key is required.")
            .MaximumLength(50).WithMessage("Key must not exceed 50 characters.");

        RuleFor(x => x.Value)
            .NotEmpty().WithMessage("Value is required.");
    }
}