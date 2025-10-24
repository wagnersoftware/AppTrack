using AppTrack.Application.Contracts.Persistance;
using AppTrack.Application.Features.AiSettings.Dto;
using FluentValidation;
using System.Text.RegularExpressions;

namespace AppTrack.Application.Features.AiSettings.Commands.UpdateAiSettings;

public class UpdateAiSettingsCommandValidator : AbstractValidator<UpdateAiSettingsCommand>
{
    private readonly IAiSettingsRepository _aiSettingsRepository;

    public UpdateAiSettingsCommandValidator(IAiSettingsRepository aiSettingsRepository)
    {
        this._aiSettingsRepository = aiSettingsRepository;

        RuleFor(x => x.Id)
        .GreaterThan(0).WithMessage("{PropertyName} must be greater than 0.")
        .NotEmpty().WithMessage("{PropertyName} is required");

        RuleFor(x => x.UserId)
        .NotEmpty().WithMessage("UserId is required")
        .Matches("^[a-zA-Z0-9\\-]+$").WithMessage("UserId contains invalid characters.");

        RuleFor(x => x.ApiKey)
        .MaximumLength(200).WithMessage("ApiKey must not exceed 200 characters.")
        .Must(apiKey => string.IsNullOrEmpty(apiKey) || Regex.IsMatch(apiKey, "^sk-[A-Za-z0-9]{20,}$"))
        .WithMessage("ApiKey must be empty or a valid OpenAI API key.");

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

        RuleForEach(x => x.PromptParameter)
        .SetValidator(new PromptParameterDtoValidator());

        RuleFor(x => x.PromptParameter)
        .Must(HaveUniqueKeys)
        .WithMessage("Each prompt parameter key must be unique.");

    }

    private static bool HaveUniqueKeys(List<PromptParameterDto> parameters)
    {
        if (parameters == null || parameters.Count == 0)
            return true;

        var duplicateKeys = parameters
            .GroupBy(p => p.Key)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();

        return duplicateKeys.Count == 0;
    }

}
