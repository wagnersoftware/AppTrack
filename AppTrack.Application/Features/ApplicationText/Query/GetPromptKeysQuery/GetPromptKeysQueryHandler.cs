using AppTrack.Application.Contracts.Mediator;
using AppTrack.Application.Contracts.Persistance;
using AppTrack.Application.Exceptions;
using AppTrack.Application.Features.ApplicationText.Dto;
using FluentValidation;

namespace AppTrack.Application.Features.ApplicationText.Query.GetPromptKeysQuery;

public class GetPromptKeysQueryHandler : IRequestHandler<GetPromptKeysQuery, GetPromptKeysDto>
{
    private readonly IAiSettingsRepository _aiSettingsRepository;
    private readonly IBuiltInPromptRepository _builtInPromptRepository;
    private readonly IValidator<GetPromptKeysQuery> _validator;

    public GetPromptKeysQueryHandler(IAiSettingsRepository aiSettingsRepository, IBuiltInPromptRepository builtInPromptRepository, IValidator<GetPromptKeysQuery> validator)
    {
        _aiSettingsRepository = aiSettingsRepository;
        _builtInPromptRepository = builtInPromptRepository;
        _validator = validator;
    }

    public async Task<GetPromptKeysDto> Handle(GetPromptKeysQuery request, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(request);

        if (validationResult.Errors.Count > 0)
            throw new BadRequestException("Invalid request.", validationResult);

        var aiSettings = await _aiSettingsRepository.GetByUserIdWithPromptsReadOnlyAsync(request.UserId);
        var builtInPrompts = await _builtInPromptRepository.GetAsync();

        var userPromptNames = aiSettings!.Prompts.Select(p => p.Name);
        var builtInPromptNames = builtInPrompts.Select(d => d.Name);

        var names = userPromptNames.Concat(builtInPromptNames).ToList();
        return new GetPromptKeysDto { Keys = names };
    }
}
