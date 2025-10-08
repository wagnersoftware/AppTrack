using AppTrack.Application.Contracts.Persistance;
using FluentValidation;

namespace AppTrack.Application.Features.JobApplications.Commands.GenerateApplicationText;

public class GenerateApplicationTextCommandValidator : AbstractValidator<GenerateApplicationTextCommand>
{
    private readonly IJobApplicationRepository _jobApplicationRepository;
    private readonly IAiSettingsRepository _aiSettingsRepository;

    public GenerateApplicationTextCommandValidator(IJobApplicationRepository jobApplicationRepository, IAiSettingsRepository aiSettingsRepository)
    {
        _jobApplicationRepository = jobApplicationRepository;
        _aiSettingsRepository = aiSettingsRepository;

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("{PropertyName} is required");

        RuleFor(x => x.ApplicationId)
            .NotEmpty().WithMessage("{PropertyName} is required");

        RuleFor(x => x.URL)
            .NotEmpty().WithMessage("{PropertyName} is required");

        RuleFor(x => x.Position)
            .NotEmpty().WithMessage("{PropertyName} is required");

        RuleFor(x => x)
            .MustAsync(JobApplicationExists)
            .WithMessage("Job application doesn't exist");

        RuleFor(x => x)
            .CustomAsync(ValidateAiSettings);
    }

    private async Task<bool> JobApplicationExists(GenerateApplicationTextCommand command, CancellationToken token)
    {
        var jobApplication = await _jobApplicationRepository.GetByIdAsync(command.ApplicationId);
        return jobApplication != null;
    }

    private async Task ValidateAiSettings(GenerateApplicationTextCommand command, ValidationContext<GenerateApplicationTextCommand> context, CancellationToken token)
    {
        var aiSettings = await _aiSettingsRepository.GetByUserIdWithPromptParameterAsync(command.UserId);

        if (aiSettings == null)
        {
            context.AddFailure("AI settings not found for this user.");
            return;
        }

        if (string.IsNullOrWhiteSpace(aiSettings.ApiKey))
            context.AddFailure("ApiKey in AI settings is missing.");

        if (string.IsNullOrWhiteSpace(aiSettings.Prompt))
            context.AddFailure("Prompt in AI settings is missing.");

    }
}

