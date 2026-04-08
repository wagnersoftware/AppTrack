using AppTrack.Application.Contracts.Mediator;
using AppTrack.Application.Contracts.Persistance;
using AppTrack.Application.Exceptions;
using AppTrack.Application.Features.AiSettings.Dto;
using AppTrack.Application.Mappings;
using AppTrack.Domain.Enums;

namespace AppTrack.Application.Features.AiSettings.Queries.GetAiSettingsByUserId;

public class GetAiSettingsByUserIdQueryHandler : IRequestHandler<GetAiSettingsByUserIdQuery, AiSettingsDto>
{
    private readonly IAiSettingsRepository _aiSettingsRepository;
    private readonly IDefaultPromptRepository _defaultPromptRepository;

    public GetAiSettingsByUserIdQueryHandler(IAiSettingsRepository aiSettingsRepository, IDefaultPromptRepository defaultPromptRepository)
    {
        _aiSettingsRepository = aiSettingsRepository;
        _defaultPromptRepository = defaultPromptRepository;
    }

    /// <summary>
    /// Gets the AI settings for the specified user. Creates and returns a new instance if the entity doesn't exist.
    /// </summary>
    public async Task<AiSettingsDto> Handle(GetAiSettingsByUserIdQuery request, CancellationToken cancellationToken)
    {
        var validator = new GetAiSettingsByUserIdQueryValidator();
        var validationResult = await validator.ValidateAsync(request);

        if (validationResult.Errors.Any())
            throw new BadRequestException($"Invalid request", validationResult);

        var entity = await _aiSettingsRepository.GetByUserIdIncludePromptParameterAsync(request.UserId);

        if (entity == null)
        {
            entity = request.ToDomain();
            await _aiSettingsRepository.CreateAsync(entity);
        }

        var dto = entity.ToDto();
        var languageCode = entity.Language == ApplicationLanguage.German ? "de" : "en";
        var defaults = await _defaultPromptRepository.GetByLanguageAsync(languageCode);
        dto.DefaultPrompts = defaults.Select(d => d.ToDto()).ToList();
        return dto;
    }
}
