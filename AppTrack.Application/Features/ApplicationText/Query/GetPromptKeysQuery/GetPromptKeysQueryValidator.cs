using AppTrack.Application.Contracts.Persistance;
using FluentValidation;

namespace AppTrack.Application.Features.ApplicationText.Query.GetPromptKeysQuery;

public class GetPromptKeysQueryValidator : AbstractValidator<GetPromptKeysQuery>
{
    private readonly IAiSettingsRepository _aiSettingsRepository;

    public GetPromptKeysQueryValidator(IAiSettingsRepository aiSettingsRepository)
    {
        _aiSettingsRepository = aiSettingsRepository;

        RuleFor(x => x)
            .CustomAsync(ValidateAiSettings);
    }

    private async Task ValidateAiSettings(GetPromptKeysQuery query, ValidationContext<GetPromptKeysQuery> context, CancellationToken token)
    {
        var aiSettings = await _aiSettingsRepository.GetByUserIdWithPromptsReadOnlyAsync(query.UserId);

        if (aiSettings == null)
            context.AddFailure("AI settings not found for this user.");
    }
}
