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
    private readonly IBuiltInPromptRepository _builtInPromptRepository;

    public GeneratePromptQueryHandler(
        IAiSettingsRepository aiSettingsRepository,
        IJobApplicationRepository jobApplicationRepository,
        IPromptBuilder promptBuilder,
        IBuiltInPromptRepository builtInPromptRepository)
    {
        _aiSettingsRepository = aiSettingsRepository;
        _jobApplicationRepository = jobApplicationRepository;
        _promptBuilder = promptBuilder;
        _builtInPromptRepository = builtInPromptRepository;
    }

    public async Task<GeneratedPromptDto> Handle(GeneratePromptQuery request, CancellationToken cancellationToken)
    {
        var validator = new GeneratePromptQueryValidator(_jobApplicationRepository, _aiSettingsRepository, _builtInPromptRepository);
        var validationResult = await validator.ValidateAsync(request);

        if (validationResult.Errors.Any())
            throw new BadRequestException("Invalid generate prompt request.", validationResult);

        var aiSettings = await _aiSettingsRepository.GetByUserIdWithPromptsReadOnlyAsync(request.UserId);
        var jobApplication = await _jobApplicationRepository.GetByIdAsync(request.JobApplicationId);

        var applicantParameter = aiSettings!.PromptParameter.ToList();
        var jobApplicationParameter = jobApplication!.ToPromptParameters().ToList();
        var promptParameter = jobApplicationParameter.Union(applicantParameter).ToList();

        string promptTemplate;
        if (request.PromptName.StartsWith("Default_", StringComparison.Ordinal))
        {
            var defaults = await _builtInPromptRepository.GetAsync();
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
