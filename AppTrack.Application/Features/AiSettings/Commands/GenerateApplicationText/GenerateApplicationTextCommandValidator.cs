using AppTrack.Application.Contracts.Persistance;
using FluentValidation;

namespace AppTrack.Application.Features.AiSettings.Commands.GenerateApplicationText;

public class GenerateApplicationTextCommandValidator : AbstractValidator<GenerateApplicationTextCommand>
{
    private readonly IJobApplicationRepository _jobApplicationRepository;
    private readonly IAiSettingsRepository _aiSettingsRepository;
    private readonly IChatModelRepository _chatModelRepository;

    public GenerateApplicationTextCommandValidator(
        IJobApplicationRepository jobApplicationRepository, 
        IAiSettingsRepository aiSettingsRepository,
        IChatModelRepository chatModelRepository)
    {
        _jobApplicationRepository = jobApplicationRepository;
        _aiSettingsRepository = aiSettingsRepository;
        _chatModelRepository = chatModelRepository;

        RuleFor(x => x.Prompt)
            .NotEmpty().WithMessage("{PropertyName} is required")
            .NotNull().WithMessage("{PropertyName} is required");

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

    private async Task<bool> JobApplicationExists(GenerateApplicationTextCommand command, CancellationToken token)
    {
        var jobApplication = await _jobApplicationRepository.GetByIdAsync(command.JobApplicationId);
        return jobApplication != null;
    }

    private async Task ValidateAiSettings(
        GenerateApplicationTextCommand command,
        ValidationContext<GenerateApplicationTextCommand> context,
        CancellationToken token)
    {
        var aiSettings = await _aiSettingsRepository
            .GetByUserIdWithPromptParameterAsync(command.UserId);

        if (aiSettings == null)
        {
            context.AddFailure("AI settings not found for this user.");
            return;
        }

        if (string.IsNullOrWhiteSpace(aiSettings.ApiKey))
            context.AddFailure("ApiKey in AI settings is missing.");

        var chatModel = await _chatModelRepository.GetByIdAsync(aiSettings.SelectedChatModelId);

        if (chatModel == null)
        {
            context.AddFailure("No ChatModel configured on server.");
            return;
        }

        if (chatModel!.ApiModelName == null)
        {
            context.AddFailure("No model name for chat api set.");
        }
    }
}