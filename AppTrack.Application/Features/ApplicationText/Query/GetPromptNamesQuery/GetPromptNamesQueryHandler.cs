using AppTrack.Application.Contracts.Mediator;
using AppTrack.Application.Contracts.Persistance;
using AppTrack.Application.Exceptions;
using AppTrack.Application.Features.ApplicationText.Dto;

namespace AppTrack.Application.Features.ApplicationText.Query.GetPromptNamesQuery;

public class GetPromptNamesQueryHandler : IRequestHandler<GetPromptNamesQuery, GetPromptNamesDto>
{
    private readonly IAiSettingsRepository _aiSettingsRepository;
    private readonly IBuiltInPromptRepository _builtInPromptRepository;

    public GetPromptNamesQueryHandler(IAiSettingsRepository aiSettingsRepository, IBuiltInPromptRepository builtInPromptRepository)
    {
        _aiSettingsRepository = aiSettingsRepository;
        _builtInPromptRepository = builtInPromptRepository;
    }

    public async Task<GetPromptNamesDto> Handle(GetPromptNamesQuery request, CancellationToken cancellationToken)
    {
        var validator = new GetPromptNamesQueryValidator(_aiSettingsRepository);
        var validationResult = await validator.ValidateAsync(request);

        if (validationResult.Errors.Any())
            throw new BadRequestException("Invalid request.", validationResult);

        var aiSettings = await _aiSettingsRepository.GetByUserIdIncludePromptParameterAsync(request.UserId);
        var builtInPrompts = await _builtInPromptRepository.GetAsync();

        var userPromptNames = aiSettings!.Prompts.Select(p => p.Name);
        var builtInPromptNames = builtInPrompts.Select(d => d.Name);

        var names = userPromptNames.Concat(builtInPromptNames).ToList();
        return new GetPromptNamesDto { Names = names };
    }
}
