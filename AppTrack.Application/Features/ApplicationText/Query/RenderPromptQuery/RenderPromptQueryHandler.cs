using AppTrack.Application.Contracts.Mediator;
using AppTrack.Application.Contracts.Persistance;
using AppTrack.Application.Exceptions;
using AppTrack.Application.Features.ApplicationText.Dto;
using AppTrack.Domain;
using AppTrack.Domain.Contracts;
using AppTrack.Domain.Extensions;
using FluentValidation;

namespace AppTrack.Application.Features.ApplicationText.Query.RenderPromptQuery;

public class RenderPromptQueryHandler : IRequestHandler<RenderPromptQuery, RenderedPromptDto>
{
    private readonly IAiSettingsRepository _aiSettingsRepository;
    private readonly IJobApplicationRepository _jobApplicationRepository;
    private readonly IPromptBuilder _promptBuilder;
    private readonly IBuiltInPromptRepository _builtInPromptRepository;
    private readonly IValidator<RenderPromptQuery> _validator;

    public RenderPromptQueryHandler(
        IAiSettingsRepository aiSettingsRepository,
        IJobApplicationRepository jobApplicationRepository,
        IPromptBuilder promptBuilder,
        IBuiltInPromptRepository builtInPromptRepository,
        IValidator<RenderPromptQuery> validator)
    {
        _aiSettingsRepository = aiSettingsRepository;
        _jobApplicationRepository = jobApplicationRepository;
        _promptBuilder = promptBuilder;
        _builtInPromptRepository = builtInPromptRepository;
        _validator = validator;
    }

    public async Task<RenderedPromptDto> Handle(RenderPromptQuery request, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(request);

        if (validationResult.Errors.Any())
            throw new BadRequestException("Invalid render prompt request.", validationResult);

        var aiSettings = await _aiSettingsRepository.GetByUserIdWithPromptsReadOnlyAsync(request.UserId);
        var jobApplication = await _jobApplicationRepository.GetByIdAsync(request.JobApplicationId);

        var builtInParameters = aiSettings!.BuiltInPromptParameter
            .Select(p => PromptParameter.Create(p.Key, p.Value));
        var applicantParameter = aiSettings!.PromptParameter
            .Concat(builtInParameters)
            .ToList();
        var jobApplicationParameter = jobApplication!.ToPromptParameters().ToList();
        var promptParameter = jobApplicationParameter.Union(applicantParameter).ToList();

        string promptTemplate;
        if (request.PromptKey.StartsWith(BuiltInParameterKeys.Prefix, StringComparison.Ordinal))
        {
            var defaults = await _builtInPromptRepository.GetAsync();
            promptTemplate = defaults
                .First(p => string.Equals(p.Name, request.PromptKey, StringComparison.OrdinalIgnoreCase))
                .PromptTemplate;
        }
        else
        {
            promptTemplate = aiSettings!.Prompts
                .First(p => string.Equals(p.Name, request.PromptKey, StringComparison.OrdinalIgnoreCase))
                .PromptTemplate;
        }

        var (prompt, unusedKeys) = _promptBuilder.BuildPrompt(promptParameter, promptTemplate);
        return new RenderedPromptDto() { Prompt = prompt, UnusedKeys = unusedKeys };
    }
}
