using AppTrack.Application.Contracts.ApplicationTextGenerator;
using AppTrack.Application.Contracts.Mediator;
using AppTrack.Application.Contracts.Persistance;
using AppTrack.Application.Exceptions;
using AppTrack.Application.Features.JobApplications.Dto;
using AppTrack.Domain.Contracts;

namespace AppTrack.Application.Features.AiSettings.Commands.GenerateApplicationText;

public class GenerateApplicationTextCommandHandler : IRequestHandler<GenerateApplicationTextCommand, GeneratedApplicationTextDto>
{
    private readonly IApplicationTextGenerator _applicationTextGenerator;
    private readonly IAiSettingsRepository _aiSettingsRepository;
    private readonly IJobApplicationRepository _jobApplicationRepository;
    private readonly IChatModelRepository _chatModelRepository;

    public GenerateApplicationTextCommandHandler(IApplicationTextGenerator applicationTextGenerator,
                                                 IAiSettingsRepository aiSettingsRepository,
                                                 IJobApplicationRepository jobApplicationRepository,
                                                 IChatModelRepository chatModelRepository,
                                                 IPromptBuilder promptBuilder)
    {
        this._applicationTextGenerator = applicationTextGenerator;
        this._aiSettingsRepository = aiSettingsRepository;
        this._jobApplicationRepository = jobApplicationRepository;
        this._chatModelRepository = chatModelRepository;
    }

    public async Task<GeneratedApplicationTextDto> Handle(GenerateApplicationTextCommand request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var validator = new GenerateApplicationTextCommandValidator(_jobApplicationRepository, _aiSettingsRepository, _chatModelRepository);
        var validationResult = await validator.ValidateAsync(request);

        if (validationResult.Errors.Any())
        {
            throw new BadRequestException($"Invalid Ai setting.", validationResult);
        }

        //get Ai settings
        cancellationToken.ThrowIfCancellationRequested();
        var aiSettings = await _aiSettingsRepository.GetByUserIdWithPromptParameterAsync(request.UserId);
        var chatModel= await _chatModelRepository.GetByIdAsync(aiSettings!.SelectedChatModelId);
        var chatModelName = chatModel!.ApiModelName;

        //generate application text
        _applicationTextGenerator.SetApiKey(aiSettings!.ApiKey);
        var generatedApplicationText = await _applicationTextGenerator.GenerateApplicationTextAsync(request.Prompt, chatModelName, cancellationToken);

        //update the job application with generated text
        var jobApplication = await _jobApplicationRepository.GetByIdAsync(request.JobApplicationId);
        jobApplication!.ApplicationText = generatedApplicationText;
        await _jobApplicationRepository.UpdateAsync(jobApplication);

        return new GeneratedApplicationTextDto() { ApplicationText = generatedApplicationText };
    }
}
