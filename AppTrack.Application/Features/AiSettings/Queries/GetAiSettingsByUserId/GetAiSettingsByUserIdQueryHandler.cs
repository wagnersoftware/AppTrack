using AppTrack.Application.Contracts.Mediator;
using AppTrack.Application.Contracts.Persistance;
using AppTrack.Application.Exceptions;
using AppTrack.Application.Features.AiSettings.Dto;
using AppTrack.Application.Mappings;
using FluentValidation;

namespace AppTrack.Application.Features.AiSettings.Queries.GetAiSettingsByUserId;

public class GetAiSettingsByUserIdQueryHandler : IRequestHandler<GetAiSettingsByUserIdQuery, AiSettingsDto>
{
    private readonly IAiSettingsRepository _aiSettingsRepository;
    private readonly IBuiltInPromptRepository _builtInPromptRepository;
    private readonly IValidator<GetAiSettingsByUserIdQuery> _validator;

    public GetAiSettingsByUserIdQueryHandler(IAiSettingsRepository aiSettingsRepository, IBuiltInPromptRepository builtInPromptRepository, IValidator<GetAiSettingsByUserIdQuery> validator)
    {
        _aiSettingsRepository = aiSettingsRepository;
        _builtInPromptRepository = builtInPromptRepository;
        _validator = validator;
    }

    /// <summary>
    /// Gets the AI settings for the specified user. Creates and returns a new instance if the entity doesn't exist.
    /// </summary>
    public async Task<AiSettingsDto> Handle(GetAiSettingsByUserIdQuery request, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(request);

        if (validationResult.Errors.Any())
            throw new BadRequestException($"Invalid request", validationResult);

        var entity = await _aiSettingsRepository.GetByUserIdWithPromptsReadOnlyAsync(request.UserId);

        if (entity == null)
        {
            entity = request.ToDomain();
            await _aiSettingsRepository.CreateAsync(entity);
        }

        var dto = entity.ToDto();
        var defaults = await _builtInPromptRepository.GetAsync();
        dto.BuiltInPrompts = defaults.Select(d => d.ToDto()).ToList();
        return dto;
    }
}
