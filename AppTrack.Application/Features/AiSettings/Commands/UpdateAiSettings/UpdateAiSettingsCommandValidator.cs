using AppTrack.Application.Contracts.Persistance;
using AppTrack.Shared.Validation.Validators;
using FluentValidation;

namespace AppTrack.Application.Features.AiSettings.Commands.UpdateAiSettings;

public class UpdateAiSettingsCommandValidator : AiSettingsBaseValidator<UpdateAiSettingsCommand>
{
    private readonly IAiSettingsRepository _aiSettingsRepository;

    public UpdateAiSettingsCommandValidator(IAiSettingsRepository aiSettingsRepository)
    {
        _aiSettingsRepository = aiSettingsRepository;

        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("{PropertyName} must be greater than 0.")
            .NotEmpty().WithMessage("{PropertyName} is required");

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("UserId is required")
            .Matches("^[a-zA-Z0-9\\-]+$").WithMessage("UserId contains invalid characters.");

        RuleFor(x => x)
            .CustomAsync(async (command, context, cancellationToken) =>
            {
                var aiSettings = await _aiSettingsRepository.GetByIdAsync(command.Id);
                if (aiSettings is null)
                {
                    context.AddFailure("Id", "Ai settings not found.");
                    return;
                }

                if (aiSettings.UserId != command.UserId)
                {
                    context.AddFailure("UserId", "Ai settings not assigned to this user.");
                }
            });
    }
}
