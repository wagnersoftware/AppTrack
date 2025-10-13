using AppTrack.Application.Contracts.Persistance;
using FluentValidation;

namespace AppTrack.Application.Features.ApplicationText.Query;

public class GeneratePromptQueryValidator : AbstractValidator<GeneratePromptQuery>
{
    private readonly IJobApplicationRepository _jobApplicationRepository;
    private readonly IAiSettingsRepository _aiSettingsRepository;

    public GeneratePromptQueryValidator(IJobApplicationRepository jobApplicationRepository, IAiSettingsRepository aiSettingsRepository)
    {
        _jobApplicationRepository = jobApplicationRepository;
        _aiSettingsRepository = aiSettingsRepository;

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("{PropertyName} is required")
            .NotNull().WithMessage("{PropertyName} is required");

        RuleFor(x => x.JobApplicationId)
            .NotEmpty().WithMessage("{PropertyName} is required")
            .NotNull().WithMessage("{PropertyName} is required");

        RuleFor(x => x)
            .MustAsync(JobApplicationExists)
            .WithMessage("Job application doesn't exist");

        RuleFor(x => x)
            .CustomAsync(ValidateAiSettings);
    }

    private async Task<bool> JobApplicationExists(GeneratePromptQuery query, CancellationToken token)
    {
        var jobApplication = await _jobApplicationRepository.GetByIdAsync(query.JobApplicationId);
        return jobApplication != null;
    }

    private async Task ValidateAiSettings(GeneratePromptQuery query, ValidationContext<GeneratePromptQuery> context, CancellationToken token)
    {
        var aiSettings = await _aiSettingsRepository.GetByUserIdWithPromptParameterAsync(query.UserId);

        if (aiSettings == null)
        {
            context.AddFailure("AI settings not found for this user.");
            return;
        }

        if (string.IsNullOrWhiteSpace(aiSettings.ApiKey))
            context.AddFailure("ApiKey in AI settings is missing.");

        if (string.IsNullOrWhiteSpace(aiSettings.PromptTemplate))
            context.AddFailure("Prompt in AI settings is missing.");

    }
}
