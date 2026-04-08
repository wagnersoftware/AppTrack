using AppTrack.Application.Contracts.Mediator;
using AppTrack.Application.Contracts.Persistance;
using AppTrack.Application.Exceptions;
using AppTrack.Application.Features.ApplicationText.Dto;
using AppTrack.Domain.Contracts;
using AppTrack.Domain.Enums;
using AppTrack.Domain.Extensions;

namespace AppTrack.Application.Features.ApplicationText.Query.GeneratePromptQuery;

public class GeneratePromptQueryHandler : IRequestHandler<GeneratePromptQuery, GeneratedPromptDto>
{
    private readonly IAiSettingsRepository _aiSettingsRepository;
    private readonly IJobApplicationRepository _jobApplicationRepository;
    private readonly IPromptBuilder _promptBuilder;
    private readonly IDefaultPromptRepository _defaultPromptRepository;

    public GeneratePromptQueryHandler(
        IAiSettingsRepository aiSettingsRepository,
        IJobApplicationRepository jobApplicationRepository,
        IPromptBuilder promptBuilder,
        IDefaultPromptRepository defaultPromptRepository)
    {
        _aiSettingsRepository = aiSettingsRepository;
        _jobApplicationRepository = jobApplicationRepository;
        _promptBuilder = promptBuilder;
        _defaultPromptRepository = defaultPromptRepository;
    }

    public async Task<GeneratedPromptDto> Handle(GeneratePromptQuery request, CancellationToken cancellationToken)
    {
        var validator = new GeneratePromptQueryValidator(_jobApplicationRepository, _aiSettingsRepository, _defaultPromptRepository);
        var validationResult = await validator.ValidateAsync(request);

        if (validationResult.Errors.Any())
            throw new BadRequestException("Invalid generate prompt request.", validationResult);

        var aiSettings = await _aiSettingsRepository.GetByUserIdIncludePromptParameterAsync(request.UserId);
        var jobApplication = await _jobApplicationRepository.GetByIdAsync(request.JobApplicationId);

        var applicantParameter = aiSettings!.PromptParameter.ToList();
        var jobApplicationParameter = jobApplication!.ToPromptParameters().ToList();
        var promptParameter = jobApplicationParameter.Union(applicantParameter).ToList();

        string promptTemplate;
        if (request.PromptName.StartsWith("Default_", StringComparison.Ordinal))
        {
            var languageCode = aiSettings!.Language == ApplicationLanguage.German ? "de" : "en";
            var defaults = await _defaultPromptRepository.GetByLanguageAsync(languageCode);
            promptTemplate = defaults
                .First(p => string.Equals(p.Name, request.PromptName, StringComparison.OrdinalIgnoreCase))
                .PromptTemplate;
        }
        else
        {
            promptTemplate = aiSettings!.Prompts
                .First(p => string.Equals(p.Name, request.PromptName, StringComparison.OrdinalIgnoreCase))
                .PromptTemplate;
        }

        var (prompt, unusedKeys) = _promptBuilder.BuildPrompt(promptParameter, promptTemplate);
        return new GeneratedPromptDto() { Prompt = prompt, UnusedKeys = unusedKeys };
    }
}
