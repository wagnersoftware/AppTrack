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
        .MustAsync(AiSettingsExists)
        .WithMessage("Ai setting doesn't exist");

        RuleFor(x => x)
        .MustAsync(AiSettingsExistsForUser)
        .WithMessage("The requested ai setting doesn't match the user id");
    }

    private async Task<bool> AiSettingsExists(UpdateAiSettingsCommand command, CancellationToken token)
    {
        var aiSettings = await _aiSettingsRepository.GetByIdAsync(command.Id!);
        return aiSettings != null;
    }

    private async Task<bool> AiSettingsExistsForUser(UpdateAiSettingsCommand command, CancellationToken token)
    {
        var aiSettings = await _aiSettingsRepository.GetByUserIdAsync(command.UserId!);
        return aiSettings != null;
    }
}
