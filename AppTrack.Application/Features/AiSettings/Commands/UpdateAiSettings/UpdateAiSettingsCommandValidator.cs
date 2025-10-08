using AppTrack.Application.Contracts.Persistance;
using FluentValidation;

namespace AppTrack.Application.Features.AiSettings.Commands.UpdateAiSettings;

public class UpdateAiSettingsCommandValidator : AbstractValidator<UpdateAiSettingsCommand>
{
    private readonly IAiSettingsRepository _aiSettingsRepository;

    public UpdateAiSettingsCommandValidator(IAiSettingsRepository aiSettingsRepository)
    {
        this._aiSettingsRepository = aiSettingsRepository;

        RuleFor(x => x.Id)
        .NotNull().WithMessage("{PropertyName} is required");

        RuleFor(x => x.UserId)
        .NotEmpty().WithMessage("{PropertyName} is required")
        .NotNull().WithMessage("{PropertyName} is required");

        RuleFor(x => x.Prompt)
        .NotEmpty().WithMessage("{PropertyName} is required")
        .NotNull().WithMessage("{PropertyName} is required");

        RuleFor(x => x.ApiKey)
        .NotEmpty().WithMessage("{PropertyName} is required")
        .NotNull().WithMessage("{PropertyName} is required");

        RuleFor(x => x.ApiKey)
        .NotEmpty().WithMessage("{PropertyName} is required")
        .NotNull().WithMessage("{PropertyName} is required");

        RuleFor(x => x)
            .MustAsync(AiSettingsExistsAndBelongsToUser)
            .WithMessage("Ai settings not found or not assigned to this user.");
    }

    private async Task<bool> AiSettingsExistsAndBelongsToUser(UpdateAiSettingsCommand command, CancellationToken token)
    {
        var aiSettings = await _aiSettingsRepository.GetByIdAsync(command.Id);
        if (aiSettings is null)
        {
            return false;
        }
        return aiSettings.UserId == command.UserId;
    }
}
