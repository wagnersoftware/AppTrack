using AppTrack.Application.Contracts.AiTextGenerator;
using AppTrack.Application.Contracts.Mediator;
using AppTrack.Application.Contracts.Persistance;
using AppTrack.Application.Exceptions;
using AppTrack.Application.Features.JobApplications.Dto;
using AppTrack.Domain.Contracts;

namespace AppTrack.Application.Features.AiSettings.Commands.GenerateAiText;

public class GenerateAiTextCommandHandler : IRequestHandler<GenerateAiTextCommand, GeneratedAiTextDto>
{
    private readonly IAiTextGenerator _aiTextGenerator;
    private readonly IAiSettingsRepository _aiSettingsRepository;
    private readonly IJobApplicationRepository _jobApplicationRepository;
    private readonly IChatModelRepository _chatModelRepository;

    public GenerateAiTextCommandHandler(IAiTextGenerator aiTextGenerator,
                                        IAiSettingsRepository aiSettingsRepository,
                                        IJobApplicationRepository jobApplicationRepository,
                                        IChatModelRepository chatModelRepository,
                                        IPromptBuilder promptBuilder)
    {
        this._aiTextGenerator = aiTextGenerator;
        this._aiSettingsRepository = aiSettingsRepository;
        this._jobApplicationRepository = jobApplicationRepository;
        this._chatModelRepository = chatModelRepository;
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

        //get Ai settings
        cancellationToken.ThrowIfCancellationRequested();
        var aiSettings = await _aiSettingsRepository.GetByUserIdWithPromptsReadOnlyAsync(request.UserId);
        var chatModel= await _chatModelRepository.GetByIdAsync(aiSettings!.SelectedChatModelId);
        var chatModelName = chatModel!.ApiModelName;

        //generate ai text
        var generatedText = await _aiTextGenerator.GenerateAiTextAsync(request.Prompt, chatModelName, aiSettings.Language, cancellationToken);

        //update the job application with generated text
        var jobApplication = await _jobApplicationRepository.GetByIdAsync(request.JobApplicationId);
        jobApplication!.ApplicationText = generatedText;
        await _jobApplicationRepository.UpdateAsync(jobApplication);

        return new GeneratedAiTextDto() { GeneratedText = generatedText };
    }
}
