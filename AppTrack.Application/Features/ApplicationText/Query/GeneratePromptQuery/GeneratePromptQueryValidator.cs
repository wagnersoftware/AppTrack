using AppTrack.Application.Contracts.Persistance;
using FluentValidation;

namespace AppTrack.Application.Features.ApplicationText.Query.GeneratePromptQuery;

public class GeneratePromptQueryValidator : AbstractValidator<GeneratePromptQuery>
{
    private readonly IJobApplicationRepository _jobApplicationRepository;
    private readonly IAiSettingsRepository _aiSettingsRepository;

    public GeneratePromptQueryValidator(IJobApplicationRepository jobApplicationRepository, IAiSettingsRepository aiSettingsRepository)
    {
        _jobApplicationRepository = jobApplicationRepository;
        _aiSettingsRepository = aiSettingsRepository;

        RuleFor(x => x.JobApplicationId)
            .NotEmpty().WithMessage("{PropertyName} is required")
            .NotNull().WithMessage("{PropertyName} is required");

        RuleFor(x => x.PromptName)
            .NotEmpty().WithMessage("{PropertyName} is required");

        RuleFor(x => x)
            .CustomAsync(async (query, context, token) =>
            {
                var jobApplication = await _jobApplicationRepository.GetByIdAsync(query.JobApplicationId);
                if (jobApplication == null)
                {
                    context.AddFailure("Job application doesn't exist");
                    return;
                }

                if (jobApplication.UserId != query.UserId)
                {
                    context.AddFailure("Job application doesn't belong to this user.");
                }
            });

        RuleFor(x => x)
            .CustomAsync(ValidateAiSettings);
    }

    private async Task ValidateAiSettings(GeneratePromptQuery query, ValidationContext<GeneratePromptQuery> context, CancellationToken token)
    {
        var aiSettings = await _aiSettingsRepository.GetByUserIdIncludePromptParameterAsync(query.UserId);

        if (aiSettings == null)
        {
            context.AddFailure("AI settings not found for this user.");
            return;
        }

        var prompt = aiSettings.Prompts.FirstOrDefault(p => p.Name == query.PromptName);

        if (prompt == null)
        {
            context.AddFailure("Prompt not found in AI settings.");
            return;
        }

        if (string.IsNullOrWhiteSpace(prompt.PromptTemplate))
            context.AddFailure("Prompt template is empty.");
    }
}
