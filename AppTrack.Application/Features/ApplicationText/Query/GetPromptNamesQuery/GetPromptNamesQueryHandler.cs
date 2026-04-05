using AppTrack.Application.Contracts.Mediator;
using AppTrack.Application.Contracts.Persistance;
using AppTrack.Application.Exceptions;
using AppTrack.Application.Features.ApplicationText.Dto;

namespace AppTrack.Application.Features.ApplicationText.Query.GetPromptNamesQuery;

public class GetPromptNamesQueryHandler : IRequestHandler<GetPromptNamesQuery, GetPromptNamesDto>
{
    private readonly IAiSettingsRepository _aiSettingsRepository;
    private readonly IDefaultPromptRepository _defaultPromptRepository;

    public GetPromptNamesQueryHandler(IAiSettingsRepository aiSettingsRepository, IDefaultPromptRepository defaultPromptRepository)
    {
        _aiSettingsRepository = aiSettingsRepository;
        _defaultPromptRepository = defaultPromptRepository;
    }

    public async Task<GetPromptNamesDto> Handle(GetPromptNamesQuery request, CancellationToken cancellationToken)
    {
        var validator = new GetPromptNamesQueryValidator(_aiSettingsRepository);
        var validationResult = await validator.ValidateAsync(request);

        if (validationResult.Errors.Any())
            throw new BadRequestException("Invalid request.", validationResult);

        var aiSettings = await _aiSettingsRepository.GetByUserIdIncludePromptParameterAsync(request.UserId);
        var defaults = await _defaultPromptRepository.GetAsync();

        var userNames = aiSettings!.Prompts.Select(p => p.Name);
        var defaultNames = defaults.Select(d => d.Name);

        var names = userNames.Concat(defaultNames).ToList();
        return new GetPromptNamesDto { Names = names };
    }
}
