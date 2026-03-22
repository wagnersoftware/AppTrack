using AppTrack.Application.Contracts.Persistance;
using FluentValidation;

namespace AppTrack.Application.Features.ApplicationText.Query.GetPromptNamesQuery;

public class GetPromptNamesQueryValidator : AbstractValidator<GetPromptNamesQuery>
{
    private readonly IAiSettingsRepository _aiSettingsRepository;

    public GetPromptNamesQueryValidator(IAiSettingsRepository aiSettingsRepository)
    {
        _aiSettingsRepository = aiSettingsRepository;

        RuleFor(x => x)
            .CustomAsync(ValidateAiSettings);
    }

    private async Task ValidateAiSettings(GetPromptNamesQuery query, ValidationContext<GetPromptNamesQuery> context, CancellationToken token)
    {
        var aiSettings = await _aiSettingsRepository.GetByUserIdIncludePromptParameterAsync(query.UserId);

        if (aiSettings == null)
            context.AddFailure("AI settings not found for this user.");
    }
}
