using AppTrack.Application.Contracts.AiTextGenerator;
using AppTrack.Application.Contracts.Mediator;
using AppTrack.Application.Contracts.Persistance;
using AppTrack.Application.Exceptions;
using AppTrack.Application.Features.JobApplications.Dto;
using AppTrack.Domain;

namespace AppTrack.Application.Features.AiSettings.Commands.GenerateAiText;

public class GenerateAiTextCommandHandler : IRequestHandler<GenerateAiTextCommand, GeneratedAiTextDto>
{
    private readonly IAiTextGenerator _aiTextGenerator;
    private readonly IAiSettingsRepository _aiSettingsRepository;
    private readonly IJobApplicationRepository _jobApplicationRepository;
    private readonly IChatModelRepository _chatModelRepository;
    private readonly IJobApplicationAiTextRepository _aiTextRepository;

    public GenerateAiTextCommandHandler(IAiTextGenerator aiTextGenerator,
                                        IAiSettingsRepository aiSettingsRepository,
                                        IJobApplicationRepository jobApplicationRepository,
                                        IChatModelRepository chatModelRepository,
                                        IJobApplicationAiTextRepository aiTextRepository)
    {
        _aiTextGenerator = aiTextGenerator;
        _aiSettingsRepository = aiSettingsRepository;
        _jobApplicationRepository = jobApplicationRepository;
        _chatModelRepository = chatModelRepository;
        _aiTextRepository = aiTextRepository;
    }

    public async Task<GeneratedAiTextDto> Handle(GenerateAiTextCommand request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var validator = new GenerateAiTextCommandValidator(_jobApplicationRepository, _aiSettingsRepository, _chatModelRepository);
        var validationResult = await validator.ValidateAsync(request);

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
