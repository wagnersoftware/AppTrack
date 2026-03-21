using AppTrack.Application.Contracts.Mediator;
using AppTrack.Application.Contracts.Persistance;
using AppTrack.Application.Exceptions;
using AppTrack.Application.Features.ApplicationText.Dto;
using AppTrack.Domain.Contracts;
using AppTrack.Domain.Extensions;

namespace AppTrack.Application.Features.ApplicationText.Query.GeneratePromptQuery;

public class GeneratePromptQueryHandler : IRequestHandler<GeneratePromptQuery, GeneratedPromptDto>
{
    private readonly IAiSettingsRepository _aiSettingsRepository;
    private readonly IJobApplicationRepository _jobApplicationRepository;
    private readonly IPromptBuilder _promptBuilder;

    public GeneratePromptQueryHandler(IAiSettingsRepository aiSettingsRepository, IJobApplicationRepository jobApplicationRepository, IPromptBuilder promptBuilder)
    {
        this._aiSettingsRepository = aiSettingsRepository;
        this._jobApplicationRepository = jobApplicationRepository;
        this._promptBuilder = promptBuilder;
    }

    public async Task<GeneratedPromptDto> Handle(GeneratePromptQuery request, CancellationToken cancellationToken)
    {
        var validator = new GeneratePromptQueryValidator(_jobApplicationRepository, _aiSettingsRepository);
        var validationResult = await validator.ValidateAsync(request);

        if (validationResult.Errors.Any())
        {
            throw new BadRequestException($"Invalid Ai setting.", validationResult);
        }

        //get Ai settings
        var aiSettings = await _aiSettingsRepository.GetByUserIdIncludePromptParameterAsync(request.UserId);

        //get job application
        var jobApplication = await _jobApplicationRepository.GetByIdAsync(request.JobApplicationId);

        //build prompt
        var applicantParameter = aiSettings!.PromptParameter.ToList();
        var jobApplicationParameter = jobApplication!.ToPromptParameters().ToList();
        var promptParameter = jobApplicationParameter.Union(applicantParameter).ToList();
        var promptTemplate = aiSettings!.Prompts.FirstOrDefault()?.PromptTemplate ?? string.Empty;
        var (prompt, unusedKeys) = _promptBuilder.BuildPrompt(promptParameter, promptTemplate);

        return new GeneratedPromptDto() { Prompt = prompt, UnusedKeys = unusedKeys };
    }
}
