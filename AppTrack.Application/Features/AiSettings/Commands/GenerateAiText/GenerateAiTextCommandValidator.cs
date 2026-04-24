using AppTrack.Application.Contracts.Persistance;
using FluentValidation;

namespace AppTrack.Application.Features.AiSettings.Commands.GenerateAiText;

public class GenerateAiTextCommandValidator : AbstractValidator<GenerateAiTextCommand>
{
    private readonly IJobApplicationRepository _jobApplicationRepository;
    private readonly IAiSettingsRepository _aiSettingsRepository;
    private readonly IChatModelRepository _chatModelRepository;

    public GenerateAiTextCommandValidator(
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

        RuleFor(x => x.PromptKey)
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

    private async Task<bool> JobApplicationExists(GenerateAiTextCommand command, CancellationToken token)
    {
        var jobApplication = await _jobApplicationRepository.GetByIdAsync(command.JobApplicationId);
        return jobApplication != null;
    }

    private async Task ValidateAiSettings(
        GenerateAiTextCommand command,
        ValidationContext<GenerateAiTextCommand> context,
        CancellationToken token)
    {
        var aiSettings = await _aiSettingsRepository
            .GetByUserIdWithPromptsReadOnlyAsync(command.UserId);

        if (aiSettings == null)
        {
            context.AddFailure("AI settings not found for this user.");
            return;
        }

        var chatModel = await _chatModelRepository.GetByIdAsync(aiSettings.SelectedChatModelId);

        if (chatModel == null)
        {
            context.AddFailure("No ChatModel configured on server.");
            return;
        }

        if (!chatModel.IsActive)
        {
            context.AddFailure("The configured chat model is deprecated. Please select an active model in your AI settings.");
            return;
        }

        if (chatModel.ApiModelName == null)
        {
            context.AddFailure("No model name for chat api set.");
        }
    }
}
