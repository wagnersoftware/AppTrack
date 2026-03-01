using AppTrack.Application.Features.AiSettings.Dto;
using AppTrack.Shared.Validation.Validators;

namespace AppTrack.Application.Features.AiSettings.Commands.UpdateAiSettings;

public class PromptParameterDtoValidator : PromptParameterBaseValidator<PromptParameterDto>
{
    public PromptParameterDtoValidator() { }
}
