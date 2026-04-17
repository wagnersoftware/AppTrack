using AppTrack.Application.Contracts.AiTextGenerator;
using AppTrack.Application.Contracts.Mediator;
using AppTrack.Application.Contracts.Persistance;
using AppTrack.Application.Exceptions;
using AppTrack.Application.Features.JobApplications.Dto;
using AppTrack.Domain;
using FluentValidation;

namespace AppTrack.Application.Features.AiSettings.Commands.GenerateAiText;

public class GenerateAiTextCommandHandler : IRequestHandler<GenerateAiTextCommand, GeneratedAiTextDto>
{
    private readonly IAiTextGenerator _aiTextGenerator;
    private readonly IAiSettingsRepository _aiSettingsRepository;
    private readonly IChatModelRepository _chatModelRepository;
    private readonly IJobApplicationAiTextRepository _aiTextRepository;
    private readonly IValidator<GenerateAiTextCommand> _validator;

    public GenerateAiTextCommandHandler(IAiTextGenerator aiTextGenerator,
                                        IAiSettingsRepository aiSettingsRepository,
                                        IChatModelRepository chatModelRepository,
                                        IJobApplicationAiTextRepository aiTextRepository,
                                        IValidator<GenerateAiTextCommand> validator)
    {
        _aiTextGenerator = aiTextGenerator;
        _aiSettingsRepository = aiSettingsRepository;
        _chatModelRepository = chatModelRepository;
        _aiTextRepository = aiTextRepository;
        _validator = validator;
    }

    public async Task<GeneratedAiTextDto> Handle(GenerateAiTextCommand request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var validationResult = await _validator.ValidateAsync(request);

        if (validationResult.Errors.Count > 0)
        {
            throw new BadRequestException($"Invalid Ai setting.", validationResult);
        }

        cancellationToken.ThrowIfCancellationRequested();
        var aiSettings = await _aiSettingsRepository.GetByUserIdWithPromptsReadOnlyAsync(request.UserId);
        var chatModel = await _chatModelRepository.GetByIdAsync(aiSettings!.SelectedChatModelId);
        var chatModelName = chatModel!.ApiModelName;

        var generatedText = await _aiTextGenerator.GenerateAiTextAsync(request.Prompt, chatModelName, aiSettings.Language, cancellationToken);

        // Enforce max 5 history entries per (JobApplicationId, PromptKey)
        var count = await _aiTextRepository.CountByJobApplicationAndPromptAsync(request.JobApplicationId, request.PromptKey);
        if (count >= 5)
        {
            var oldest = await _aiTextRepository.GetOldestByJobApplicationAndPromptAsync(request.JobApplicationId, request.PromptKey);
            if (oldest != null)
                await _aiTextRepository.DeleteAsync(oldest);
        }

        await _aiTextRepository.AddAsync(new JobApplicationAiText
        {
            JobApplicationId = request.JobApplicationId,
            PromptKey = request.PromptKey,
            GeneratedText = generatedText,
            GeneratedAt = DateTime.UtcNow,
        });

        return new GeneratedAiTextDto { GeneratedText = generatedText };
    }
}
