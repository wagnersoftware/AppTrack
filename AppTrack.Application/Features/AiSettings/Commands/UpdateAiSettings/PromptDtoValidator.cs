using AppTrack.Application.Features.AiSettings.Dto;
using AppTrack.Shared.Validation.Validators;

namespace AppTrack.Application.Features.AiSettings.Commands.UpdateAiSettings;

public class PromptDtoValidator : PromptBaseValidator<PromptDto>
{
    public PromptDtoValidator() : base() { }
}
